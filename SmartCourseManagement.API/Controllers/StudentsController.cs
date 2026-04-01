using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Exposes paginated student listing. Admin and Instructor roles can view student lists.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin,Instructor")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get all students with pagination. Supports ?page=1&amp;pageSize=10&amp;searchTerm=alice
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<StudentReadDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _studentService.GetAllStudentsAsync(request);
            return Ok(result);
        }

        /// <summary>Get a student by ID. Admin and Instructor only.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(StudentReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound(new { message = $"Student {id} not found." });
            return Ok(student);
        }
    }
}
