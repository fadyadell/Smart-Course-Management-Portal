using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCourseManagement.API.DTOs
{
    public class UserRegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public SmartCourseManagement.API.DTOs.UserReadDto User { get; set; }
    }
}
