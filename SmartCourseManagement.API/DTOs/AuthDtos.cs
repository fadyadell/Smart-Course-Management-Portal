using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.DTOs
{
    // DTO for user registration
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|Instructor|Student)$",
            ErrorMessage = "Role must be Admin, Instructor, or Student")]
        public string Role { get; set; } = string.Empty;
    }

    // DTO for user login
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    // DTO for refreshing an expired JWT using a refresh token
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    // DTO returned for authenticated user info
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // DTO returned after successful register/login/refresh
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserReadDto User { get; set; } = null!;
    }

    // DTO for admin updating a user
    public class UserUpdateDto
    {
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(100)]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200)]
        public string? Email { get; set; }

        [RegularExpression("^(Admin|Instructor|Student)$",
            ErrorMessage = "Role must be Admin, Instructor, or Student")]
        public string? Role { get; set; }
    }
}
