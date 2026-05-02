using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/enrollments")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>Get the current student's enrollments (extracted from JWT token).</summary>
        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(EnrollmentReadDto[]), 200)]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(userId);
            return Ok(enrollments);
        }

        /// <summary>
        /// Get all enrollments for a specific student (Admin/Instructor can view any student).
        /// </summary>
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(EnrollmentReadDto[]), 200)]
        public async Task<IActionResult> GetStudentEnrollments(int studentId)
        {
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);
            return Ok(enrollments);
        }

        /// <summary>Enroll a student in a course. Student role only — enforces self-enrollment.</summary>
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

            // Security: a student can only enroll themselves (not other students)
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

        /// <summary>Remove an enrollment by ID. Admin or Student.</summary>
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
