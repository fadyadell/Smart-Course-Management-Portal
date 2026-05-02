using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>Interface for student read operations with pagination support.</summary>
    public interface IStudentService
    {
        Task<PagedResponse<StudentReadDto>> GetAllStudentsAsync(PagedRequest request);
        Task<StudentReadDto?> GetStudentByIdAsync(int id);
    }
}
