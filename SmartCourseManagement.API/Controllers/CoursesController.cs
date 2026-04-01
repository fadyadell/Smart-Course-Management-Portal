using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Course CRUD operations with pagination, search and filtering.
    /// GET endpoints require any authenticated user.
    /// POST/PUT require Admin or Instructor. DELETE requires Admin.
    /// Deleted courses are soft-deleted (IsDeleted = true) and remain in the DB.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/courses")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Get paginated list of courses. Supports search, filter by instructor/credits, and sorting.
        /// </summary>
        /// <param name="query">page, pageSize, search, instructorId, credits, sortBy, sortDesc</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CourseReadDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] CourseQueryParams query)
        {
            var result = await _courseService.GetCoursesAsync(query);
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

        /// <summary>
        /// Soft-delete a course (sets IsDeleted = true). Admin only.
        /// The record is retained in the database for history purposes.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            if (!result) return NotFound(new { message = $"Course {id} not found." });
            return NoContent();
        }
    }
}
