using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>Interface for enrollment operations with pagination support.</summary>
    public interface IEnrollmentService
    {
        Task<PagedResult<EnrollmentReadDto>> GetStudentEnrollmentsAsync(int studentId, PagedRequest request);
        Task<PagedResult<EnrollmentReadDto>> GetAllEnrollmentsAsync(EnrollmentFilterRequest filter);
        Task<PagedResult<EnrollmentReadDto>> GetInstructorCoursesEnrollmentsAsync(int instructorUserId, PagedRequest request);
        Task<EnrollmentReadDto> EnrollStudentAsync(EnrollmentCreateDto enrollmentDto);
        Task<bool> UnenrollStudentAsync(int id);
        Task<bool> UnenrollStudentByInstructorAsync(int enrollmentId, int instructorUserId);
    }
}
