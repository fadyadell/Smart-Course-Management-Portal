using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for authentication operations: register, login, and token refresh.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
