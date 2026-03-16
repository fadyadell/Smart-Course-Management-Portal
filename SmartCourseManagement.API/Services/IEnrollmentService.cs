using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollmentReadDto>> GetStudentEnrollmentsAsync(int studentId);
        Task<EnrollmentReadDto> EnrollStudentAsync(EnrollmentCreateDto enrollmentDto);
        Task<bool> UnenrollStudentAsync(int id);
    }
}
