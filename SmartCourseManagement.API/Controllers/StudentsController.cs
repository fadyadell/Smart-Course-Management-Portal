using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Exposes student listing. Admin and Instructor roles can view student lists.
    /// Students are users with Role = "Student".
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

        /// <summary>Get all students. Admin and Instructor only.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(StudentReadDto[]), 200)]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
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
