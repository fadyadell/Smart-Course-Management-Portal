using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for course CRUD operations with pagination and filtering support.
    /// </summary>
    public interface ICourseService
    {
        Task<PagedResponse<CourseReadDto>> GetAllCoursesAsync(CourseFilterRequest filter);
        Task<CourseReadDto?> GetCourseByIdAsync(int id);
        Task<CourseReadDto> CreateCourseAsync(CourseCreateDto courseDto);
        Task<bool> UpdateCourseAsync(int id, CourseUpdateDto courseDto);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> HardDeleteCourseAsync(int id);
    }
}
