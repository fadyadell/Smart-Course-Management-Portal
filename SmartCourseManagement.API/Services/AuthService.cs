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
    /// Handles user registration, login, JWT token generation, and refresh token management.
    /// Injected via DI — does NOT expose DbContext to controllers.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>Registers a new user and returns a JWT token + refresh token.</summary>
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

            var token = GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new UserReadDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        /// <summary>Authenticates a user and returns a JWT token + refresh token if credentials are valid.</summary>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Use AsNoTracking for read-only query
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new UserReadDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        /// <summary>
        /// Validates a refresh token and issues a new JWT + refresh token pair.
        /// The old refresh token is revoked (rotation).
        /// </summary>
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            // Revoke old token
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            var user = storedToken.User;
            var newToken = GenerateJwtToken(user);
            var newRefreshToken = await CreateRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                User = new UserReadDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
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
        /// Generates a JWT token containing NameIdentifier, Email, Name, and Role claims.
        /// Expiry: 15 minutes (as per enterprise token refresh pattern).
        /// </summary>
        private string GenerateJwtToken(User user)
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

            var expiry = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 15;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>Creates and stores a new refresh token for the given user (7-day expiry).</summary>
        private async Task<string> CreateRefreshTokenAsync(int userId)
        {
            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var tokenString = Convert.ToBase64String(tokenBytes);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = tokenString,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return tokenString;
        }
    }
}
