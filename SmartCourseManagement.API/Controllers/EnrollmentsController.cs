using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Manages student enrollment in courses (Many-to-Many relationship).
    /// Supports pagination on all GET endpoints.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>
        /// Get the current student's enrollments with pagination.
        /// Supports ?page=1&amp;pageSize=10
        /// </summary>
        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(PagedResponse<EnrollmentReadDto>), 200)]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] PagedRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var result = await _enrollmentService.GetStudentEnrollmentsAsync(userId, request);
            return Ok(result);
        }

        /// <summary>Get all enrollments for a specific student. Admin/Instructor only.</summary>
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(PagedResponse<EnrollmentReadDto>), 200)]
        public async Task<IActionResult> GetStudentEnrollments(int studentId, [FromQuery] PagedRequest request)
        {
            var result = await _enrollmentService.GetStudentEnrollmentsAsync(studentId, request);
            return Ok(result);
        }

        /// <summary>Get all enrollments with optional filtering. Admin/Instructor only.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(PagedResponse<EnrollmentReadDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] EnrollmentFilterRequest filter)
        {
            var result = await _enrollmentService.GetAllEnrollmentsAsync(filter);
            return Ok(result);
        }

        /// <summary>Enroll a student in a course. Student role only (self-enrollment).</summary>
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(EnrollmentReadDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto enrollmentDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);

            if (userId != enrollmentDto.StudentId)
                return Forbid();

            try
            {
                var enrollment = await _enrollmentService.EnrollStudentAsync(enrollmentDto);
                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Remove an enrollment by ID (soft delete). Admin or Student.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Student")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Unenroll(int id)
        {
            var result = await _enrollmentService.UnenrollStudentAsync(id);
            if (!result) return NotFound(new { message = $"Enrollment {id} not found." });
            return NoContent();
        }
    }
}
