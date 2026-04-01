using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles user registration, login, JWT token generation, and refresh token management.
    /// Injected via DI — does NOT expose DbContext to controllers.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>Registers a new user, sends a welcome email, and returns JWT + refresh token.</summary>
        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", SanitizeForLog(registerDto.Email));

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                _logger.LogWarning("Registration failed – duplicate email: {Email}", SanitizeForLog(registerDto.Email));
                throw new Exception("A user with this email already exists.");
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = registerDto.Role
            };

            _context.Users.Add(user);

            if (user.Role == "Instructor")
            {
                _context.InstructorProfiles.Add(new InstructorProfile
                {
                    User = user,
                    Biography = string.Empty,
                    OfficeLocation = string.Empty
                });
            }

            await _context.SaveChangesAsync();

            // Send welcome email asynchronously (fire and forget; errors are logged)
            try { await _emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
            catch (Exception ex) { _logger.LogWarning(ex, "Welcome email failed for user ID {UserId}", user.Id); }

            var (jwtToken, refreshTokenValue) = await CreateTokensAsync(user);

            _logger.LogInformation("Registration successful for user ID {UserId}", user.Id);

            return BuildAuthResponse(user, jwtToken, refreshTokenValue);
        }

        /// <summary>Authenticates a user and returns JWT + refresh token.</summary>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", SanitizeForLog(loginDto.Email));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}", SanitizeForLog(loginDto.Email));
                return null!;
            }

            var (jwtToken, refreshTokenValue) = await CreateTokensAsync(user);

            _logger.LogInformation("Login successful for user ID {UserId}", user.Id);

            return BuildAuthResponse(user, jwtToken, refreshTokenValue);
        }

        /// <summary>
        /// Exchanges a valid, unused refresh token for a new JWT + new refresh token.
        /// The old refresh token is invalidated (IsUsed = true) to prevent replay attacks.
        /// </summary>
        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh token exchange requested");

            var storedToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked
                || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token presented");
                return null;
            }

            // Invalidate the old token (single-use)
            storedToken.IsUsed = true;
            await _context.SaveChangesAsync();

            var user = storedToken.User;
            var (jwtToken, newRefreshTokenValue) = await CreateTokensAsync(user);

            _logger.LogInformation("Refresh token exchange successful for user ID {UserId}", user.Id);

            return BuildAuthResponse(user, jwtToken, newRefreshTokenValue);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);

        /// <summary>Creates a JWT + persists a new refresh token. Returns both values.</summary>
        private async Task<(string jwtToken, string refreshTokenValue)> CreateTokensAsync(User user)
        {
            var jwtToken = GenerateJwtToken(user);
            var refreshTokenValue = GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return (jwtToken, refreshTokenValue);
        }

        private static AuthResponseDto BuildAuthResponse(User user, string jwtToken, string refreshToken) =>
            new AuthResponseDto
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                User = new UserReadDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            };

        /// <summary>
        /// Generates a JWT token containing NameIdentifier, Email, Name, and Role claims.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>Generates a cryptographically secure random refresh token string.</summary>
        private static string GenerateRefreshTokenValue()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Removes newlines and carriage returns from user-supplied strings before they are written
        /// to log entries, preventing log forging / log injection attacks.
        /// </summary>
        private static string SanitizeForLog(string value) =>
            value.Replace('\n', '_').Replace('\r', '_');
    }
}
