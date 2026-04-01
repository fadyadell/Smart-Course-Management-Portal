using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Manages instructor profiles (One-to-One with User, One-to-Many with Courses).
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorService _instructorService;

        public InstructorsController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        /// <summary>Get all instructor profiles.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(InstructorReadDto[]), 200)]
        public async Task<IActionResult> GetAll()
        {
            var instructors = await _instructorService.GetAllInstructorsAsync();
            return Ok(instructors);
        }

        /// <summary>Get an instructor profile by ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InstructorReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var instructor = await _instructorService.GetInstructorByIdAsync(id);
            if (instructor == null) return NotFound(new { message = $"Instructor {id} not found." });
            return Ok(instructor);
        }

        /// <summary>Update the currently logged-in instructor's biography and office location.</summary>
        [HttpPut("profile")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile([FromBody] InstructorProfileUpdateDto profileDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var result = await _instructorService.UpdateInstructorProfileAsync(userId, profileDto);
            if (!result) return NotFound(new { message = "Instructor profile not found for this user." });
            return NoContent();
        }
    }
}
