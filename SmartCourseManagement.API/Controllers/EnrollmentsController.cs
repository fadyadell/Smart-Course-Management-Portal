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
        [ProducesResponseType(typeof(PagedResult<EnrollmentReadDto>), 200)]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] PagedRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(userId, request);
            return Ok(enrollments);
        }

        /// <summary>
        /// Get all enrollments for a specific student (Admin/Instructor can view any student).
        /// </summary>
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(PagedResult<EnrollmentReadDto>), 200)]
        public async Task<IActionResult> GetStudentEnrollments(int studentId, [FromQuery] PagedRequest request)
        {
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId, request);
            return Ok(enrollments);
        }

        /// <summary>
        /// Get all enrollments for the logged-in instructor's courses.
        /// </summary>
        [HttpGet("instructor/my-courses-enrollments")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(PagedResult<EnrollmentReadDto>), 200)]
        public async Task<IActionResult> GetMyCoursesEnrollments([FromQuery] PagedRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);

            // Get all enrollments for courses taught by this instructor
            var enrollments = await _enrollmentService.GetInstructorCoursesEnrollmentsAsync(userId, request);
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

        /// <summary>Remove an enrollment by ID. Admin, Instructor (from own course), or Student.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Student,Instructor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Unenroll(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            try
            {
                bool result;
                if (userRole == "Instructor")
                {
                    // Instructor can only drop students from their own courses
                    result = await _enrollmentService.UnenrollStudentByInstructorAsync(id, userId);
                }
                else
                {
                    // Admin and Student use the general method
                    result = await _enrollmentService.UnenrollStudentAsync(id);
                }

                if (!result) return NotFound(new { message = $"Enrollment {id} not found." });
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }
    }
}
