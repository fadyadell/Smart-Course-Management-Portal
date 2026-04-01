using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Manages course CRUD operations.
    /// - GET endpoints: accessible by all authenticated users
    /// - POST/PUT: Admin or Instructor only
    /// - DELETE: Admin only
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Require authentication for all endpoints in this controller
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>Get all courses. Requires any authenticated user.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CourseReadDto[]), 200)]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        /// <summary>
        /// Get paginated courses with optional search, filter, and sort.
        /// Query parameters: page, pageSize, searchTerm, sortBy, filter (credits value)
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponse<CourseReadDto>), 200)]
        public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
        {
            var result = await _courseService.GetCoursesAsync(request);
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

        /// <summary>Create a new course. Only Admin or Instructor roles.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(CourseReadDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CourseCreateDto courseDto)
        {
            var course = await _courseService.CreateCourseAsync(courseDto);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        /// <summary>Update a course. Only Admin or Instructor roles.</summary>
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

        /// <summary>Delete a course. Admin only.</summary>
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
