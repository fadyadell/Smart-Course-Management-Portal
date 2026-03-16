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
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDbContext _context;

        public EnrollmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EnrollmentReadDto>> GetStudentEnrollmentsAsync(int studentId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId)
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
        }

        public async Task<EnrollmentReadDto> EnrollStudentAsync(EnrollmentCreateDto enrollmentDto)
        {
            // Check if already enrolled
            if (await _context.Enrollments.AnyAsync(e => e.StudentId == enrollmentDto.StudentId && e.CourseId == enrollmentDto.CourseId))
                throw new Exception("Student already enrolled in this course");

            var enrollment = new SmartCourseManagement.API.Models.Enrollment
            {
                StudentId = enrollmentDto.StudentId,
                CourseId = enrollmentDto.CourseId
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

            return result;
        }

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
