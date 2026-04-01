using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles user registration, login, and JWT token generation with refresh token support.
    /// Access tokens expire in 15 minutes, refresh tokens expire in 7 days.
    /// Injected via DI — does NOT expose DbContext to controllers.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const int AccessTokenExpirationMinutes = 15;
        private const int RefreshTokenExpirationDays = 7;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>Registers a new user and returns JWT + refresh token.</summary>
        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
            // Check for duplicate email
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("A user with this email already exists.");

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = registerDto.Role
            };

            _context.Users.Add(user);

            // Auto-create InstructorProfile when registering as an Instructor
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

            var (accessToken, expiryDate) = GenerateJwtToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = expiryDate,
                User = new UserReadDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        /// <summary>Authenticates a user and returns JWT + refresh token if credentials are valid.</summary>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Use AsNoTracking for read-only query
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            var (accessToken, expiryDate) = GenerateJwtToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = expiryDate,
                User = new UserReadDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        /// <summary>Validates a refresh token and returns a new access token.</summary>
        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshTokenString)
        {
            // Find the refresh token in the database
            var refreshToken = await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenString && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow)
                throw new Exception("Invalid or expired refresh token.");

            // Get the user
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId);

            if (user == null)
                throw new Exception("User associated with refresh token not found.");

            // Generate a new access token
            var (accessToken, expiryDate) = GenerateJwtToken(user);

            return new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiry = expiryDate
            };
        }

        /// <summary>Hashes a plain-text password using BCrypt.</summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>Verifies a plain-text password against a BCrypt hash.</summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        /// <summary>
        /// Generates a JWT access token (15-minute expiration).
        /// Returns tuple of (token, expiryDate).
        /// </summary>
        private (string token, DateTime expiryDate) GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role) // Role used by [Authorize(Roles = "...")]
            };

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryDate = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiryDate);
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token and saves it to the database.
        /// Refresh tokens expire in 7 days.
        /// </summary>
        private async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
        {
            // Generate a random 64-byte token
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            // Save to database
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
                IsRevoked = false,
                IpAddress = "127.0.0.1", // TODO: Get from HttpContext in real app
                UserAgent = "API Client" // TODO: Get from HttpContext in real app
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshTokenString;
        }
    }
}
