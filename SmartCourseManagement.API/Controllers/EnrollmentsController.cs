using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;
using System.Security.Claims;

namespace SmartCourseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(userId);
            return Ok(enrollments);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll(EnrollmentCreateDto enrollmentDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);
            
            // Ensure student can only enroll themselves
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> Unenroll(int id)
        {
            var result = await _enrollmentService.UnenrollStudentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
