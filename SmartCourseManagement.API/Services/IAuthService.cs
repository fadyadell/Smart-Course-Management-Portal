using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for authentication operations: register and login.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
