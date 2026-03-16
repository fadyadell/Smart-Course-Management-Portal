using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
