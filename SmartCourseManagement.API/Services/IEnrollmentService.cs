using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>Interface for enrollment operations with pagination support.</summary>
    public interface IEnrollmentService
    {
        Task<PagedResponse<EnrollmentReadDto>> GetStudentEnrollmentsAsync(int studentId, PagedRequest request);
        Task<PagedResponse<EnrollmentReadDto>> GetAllEnrollmentsAsync(EnrollmentFilterRequest filter);
        Task<EnrollmentReadDto> EnrollStudentAsync(EnrollmentCreateDto enrollmentDto);
        Task<bool> UnenrollStudentAsync(int id);
    }
}
