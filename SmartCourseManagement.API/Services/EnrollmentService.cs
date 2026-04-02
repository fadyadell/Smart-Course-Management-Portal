using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles student enrollment and unenrollment operations.
    /// Demonstrates the Many-to-Many relationship between Students and Courses.
    /// </summary>
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDbContext _context;

        public EnrollmentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all enrollments for a given student using AsNoTracking() + Select().</summary>
        public async Task<IEnumerable<EnrollmentReadDto>> GetStudentEnrollmentsAsync(int studentId)
        {
            return await _context.Enrollments
                .AsNoTracking() // Read-only query optimization
                .Where(e => e.StudentId == studentId)
                .Select(e => new EnrollmentReadDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    EnrollmentDate = e.EnrollmentDate,
                    CreatedAt = e.CreatedAt,
                    CreatedBy = e.CreatedBy,
                    UpdatedAt = e.UpdatedAt,
                    UpdatedBy = e.UpdatedBy
                })
                .ToListAsync();
        }

        /// <summary>Enrolls a student in a course. Throws if already enrolled.</summary>
        public async Task<EnrollmentReadDto> EnrollStudentAsync(EnrollmentCreateDto enrollmentDto)
        {
            // Prevent duplicate enrollments
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

            // Return the created enrollment as a DTO using LINQ projection
            return await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Id == enrollment.Id)
                .Select(e => new EnrollmentReadDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    EnrollmentDate = e.EnrollmentDate,
                    CreatedAt = e.CreatedAt,
                    CreatedBy = e.CreatedBy,
                    UpdatedAt = e.UpdatedAt,
                    UpdatedBy = e.UpdatedBy
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>Removes an enrollment by ID. Returns false if not found.</summary>
        public async Task<bool> UnenrollStudentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return false;

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
