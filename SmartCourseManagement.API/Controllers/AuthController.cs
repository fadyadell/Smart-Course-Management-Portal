using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Handles user registration and login.
    /// Returns a JWT token that must be sent in the Authorization: Bearer header for protected endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Injected via DI — controller does NOT access DbContext directly
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user. Roles: Admin | Instructor | Student
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/register
        ///     {
        ///         "name": "Alice Smith",
        ///         "email": "alice@example.com",
        ///         "password": "Secret123",
        ///         "role": "Student"
        ///     }
        ///
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            // [ApiController] automatically returns HTTP 400 if model validation fails
            try
            {
                var response = await _authService.RegisterAsync(registerDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Login and receive a JWT token. Include the token as Authorization: Bearer {token}.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/login
        ///     {
        ///         "email": "alice@example.com",
        ///         "password": "Secret123"
        ///     }
        ///
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (response == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(response);
        }
    }
}
