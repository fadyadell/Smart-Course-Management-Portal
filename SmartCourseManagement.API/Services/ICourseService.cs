using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for course CRUD operations including pagination and filtering.
    /// </summary>
    public interface ICourseService
    {
        Task<PagedResult<CourseReadDto>> GetCoursesAsync(CourseQueryParams queryParams);
        Task<CourseReadDto?> GetCourseByIdAsync(int id);
        Task<CourseReadDto> CreateCourseAsync(CourseCreateDto courseDto);
        Task<bool> UpdateCourseAsync(int id, CourseUpdateDto courseDto);
        Task<bool> DeleteCourseAsync(int id);
    }
}
