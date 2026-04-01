using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Manages course CRUD with pagination, filtering, and soft/hard delete.
    /// - GET endpoints: any authenticated user
    /// - POST/PUT: Admin or Instructor
    /// - DELETE (soft): Admin or Instructor
    /// - DELETE (hard): Admin only
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Get all courses with pagination and filtering.
        /// Supports: ?page=1&amp;pageSize=10&amp;searchTerm=asp&amp;instructorName=smith&amp;minCredits=3&amp;sortBy=credits&amp;sortDirection=desc
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<CourseReadDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] CourseFilterRequest filter)
        {
            var result = await _courseService.GetAllCoursesAsync(filter);
            return Ok(result);
        }

        /// <summary>Get a course by ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CourseReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound(new { message = $"Course {id} not found." });
            return Ok(course);
        }

        /// <summary>Create a new course. Admin or Instructor only.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(CourseReadDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CourseCreateDto courseDto)
        {
            var course = await _courseService.CreateCourseAsync(courseDto);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        /// <summary>Update a course. Admin or Instructor only.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto courseDto)
        {
            var result = await _courseService.UpdateCourseAsync(id, courseDto);
            if (!result) return NotFound(new { message = $"Course {id} not found." });
            return NoContent();
        }

        /// <summary>Soft-delete a course (sets IsDeleted = true). Admin or Instructor.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            if (!result) return NotFound(new { message = $"Course {id} not found." });
            return NoContent();
        }

        /// <summary>Hard-delete a course permanently. Admin only.</summary>
        [HttpDelete("{id}/hard")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> HardDelete(int id)
        {
            var result = await _courseService.HardDeleteCourseAsync(id);
            if (!result) return NotFound(new { message = $"Course {id} not found." });
            return NoContent();
        }
    }
}
