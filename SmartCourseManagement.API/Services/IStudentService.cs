using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for student read operations.
    /// </summary>
    public interface IStudentService
    {
        Task<IEnumerable<StudentReadDto>> GetAllStudentsAsync();
        Task<StudentReadDto> GetStudentByIdAsync(int id);
    }
}
