using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for instructor profile operations.
    /// </summary>
    public interface IInstructorService
    {
        Task<IEnumerable<InstructorReadDto>> GetAllInstructorsAsync();
        Task<InstructorReadDto> GetInstructorByIdAsync(int id);
        Task<bool> UpdateInstructorProfileAsync(int userId, InstructorProfileUpdateDto profileDto);
    }
}
