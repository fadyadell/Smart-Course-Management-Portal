using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for course CRUD operations with pagination and filtering.
    /// </summary>
    public interface ICourseService
    {
        Task<IEnumerable<CourseReadDto>> GetAllCoursesAsync();
        Task<CourseReadDto> GetCourseByIdAsync(int id);
        Task<PaginationResponseDto<CourseReadDto>> GetCoursesPagedAsync(PaginationRequestDto paginationDto);
        Task<CourseReadDto> CreateCourseAsync(CourseCreateDto courseDto);
        Task<bool> UpdateCourseAsync(int id, CourseUpdateDto courseDto);
        Task<bool> DeleteCourseAsync(int id);
    }
}
