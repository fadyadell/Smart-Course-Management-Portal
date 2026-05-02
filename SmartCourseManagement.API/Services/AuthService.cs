using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles user registration, login, JWT generation, and refresh token lifecycle.
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

        /// <summary>Registers a new user and returns a JWT + refresh token.</summary>
        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
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

            var (jwt, refreshToken) = await GenerateTokensAsync(user);

            return new AuthResponseDto
            {
                Token = jwt,
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

        /// <summary>Authenticates a user and returns JWT + refresh token.</summary>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null!;

            var (jwt, refreshToken) = await GenerateTokensAsync(user);

            return new AuthResponseDto
            {
                Token = jwt,
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
        /// Validates a refresh token and issues a new JWT + refresh token pair (rotation).
        /// Returns null if the token is invalid, expired, or revoked.
        /// </summary>
        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
                return null;

            // Revoke the old token (rotation: each token is single-use)
            stored.IsRevoked = true;
            await _context.SaveChangesAsync();

            var (newJwt, newRefreshToken) = await GenerateTokensAsync(stored.User);

            return new AuthResponseDto
            {
                Token = newJwt,
                RefreshToken = newRefreshToken,
                User = new UserReadDto
                {
                    Id = stored.User.Id,
                    Name = stored.User.Name,
                    Email = stored.User.Email,
                    Role = stored.User.Role
                }
            };
        }

        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);

        // ── Private helpers ──────────────────────────────────────────────────────

        private async Task<(string jwt, string refreshToken)> GenerateTokensAsync(User user)
        {
            var jwt = GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);
            return (jwt, refreshToken);
        }

        private async Task<string> CreateRefreshTokenAsync(int userId)
        {
            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            await _context.SaveChangesAsync();
            return token;
        }

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
    }
}
