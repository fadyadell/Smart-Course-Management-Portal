using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles student enrollment and unenrollment operations with pagination support.
    /// </summary>
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public EnrollmentService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>Returns paginated enrollments for a given student.</summary>
        public async Task<PagedResponse<EnrollmentReadDto>> GetStudentEnrollmentsAsync(int studentId, PagedRequest request)
        {
            var query = _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.EnrollmentDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(e => new EnrollmentReadDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    EnrollmentDate = e.EnrollmentDate
                })
                .ToListAsync();

            return new PagedResponse<EnrollmentReadDto>(items, total, request.Page, request.PageSize);
        }

        /// <summary>Returns all enrollments with optional filtering and pagination (Admin/Instructor).</summary>
        public async Task<PagedResponse<EnrollmentReadDto>> GetAllEnrollmentsAsync(EnrollmentFilterRequest filter)
        {
            var query = _context.Enrollments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.StudentSearch))
            {
                var term = filter.StudentSearch.ToLower();
                query = query.Where(e =>
                    e.Student.Name.ToLower().Contains(term) ||
                    e.Student.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.CourseSearch))
            {
                var term = filter.CourseSearch.ToLower();
                query = query.Where(e => e.Course.Title.ToLower().Contains(term));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.EnrollmentDate)
                .Skip(filter.Skip)
                .Take(filter.Take)
                .Select(e => new EnrollmentReadDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    EnrollmentDate = e.EnrollmentDate
                })
                .ToListAsync();

            return new PagedResponse<EnrollmentReadDto>(items, total, filter.Page, filter.PageSize);
        }

        /// <summary>Enrolls a student in a course and sends a confirmation email.</summary>
        public async Task<EnrollmentReadDto> EnrollStudentAsync(EnrollmentCreateDto enrollmentDto)
        {
            if (await _context.Enrollments.AnyAsync(
                e => e.StudentId == enrollmentDto.StudentId && e.CourseId == enrollmentDto.CourseId))
            {
                throw new Exception("Student is already enrolled in this course.");
            }

            var enrollment = new Enrollment
            {
                StudentId = enrollmentDto.StudentId,
                CourseId = enrollmentDto.CourseId,
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Id == enrollment.Id)
                .Select(e => new EnrollmentReadDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    EnrollmentDate = e.EnrollmentDate
                })
                .FirstOrDefaultAsync();

            // Send enrollment confirmation email
            try
            {
                var student = await _context.Users.FindAsync(enrollmentDto.StudentId);
                if (student != null && result != null)
                    await _emailService.SendEnrollmentConfirmationAsync(student.Email, student.Name, result.CourseTitle);
            }
            catch { /* Email failure should not abort enrollment */ }

            return result!;
        }

        /// <summary>Soft-deletes (unenrolls) an enrollment by ID. Returns false if not found.</summary>
        public async Task<bool> UnenrollStudentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return false;

            enrollment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
