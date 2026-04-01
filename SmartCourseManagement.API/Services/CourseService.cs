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
    /// Handles all course CRUD operations with pagination, filtering, and soft-delete support.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;

        public CourseService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns courses with pagination and optional filtering by title, instructor name, and credits.
        /// Supports sorting by "title", "credits", or "instructor" (default: "title" asc).
        /// </summary>
        public async Task<PagedResponse<CourseReadDto>> GetAllCoursesAsync(CourseFilterRequest filter)
        {
            var query = _context.Courses.AsNoTracking().AsQueryable();

            // Free-text search on title or description
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(term) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)));
            }

            // Filter by instructor name
            if (!string.IsNullOrWhiteSpace(filter.InstructorName))
            {
                var instr = filter.InstructorName.ToLower();
                query = query.Where(c => c.Instructor.User.Name.ToLower().Contains(instr));
            }

            // Filter by credits range
            if (filter.MinCredits.HasValue)
                query = query.Where(c => c.Credits >= filter.MinCredits.Value);

            if (filter.MaxCredits.HasValue)
                query = query.Where(c => c.Credits <= filter.MaxCredits.Value);

            // Sorting
            query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
            {
                ("credits", "desc") => query.OrderByDescending(c => c.Credits),
                ("credits", _) => query.OrderBy(c => c.Credits),
                ("instructor", "desc") => query.OrderByDescending(c => c.Instructor.User.Name),
                ("instructor", _) => query.OrderBy(c => c.Instructor.User.Name),
                (_, "desc") => query.OrderByDescending(c => c.Title),
                _ => query.OrderBy(c => c.Title)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip(filter.Skip)
                .Take(filter.Take)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy
                })
                .ToListAsync();

            return new PagedResponse<CourseReadDto>(items, total, filter.Page, filter.PageSize);
        }

        /// <summary>Returns one course by ID using FirstOrDefaultAsync() + AsNoTracking().</summary>
        public async Task<CourseReadDto?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>Creates a new course and returns the created CourseReadDto.</summary>
        public async Task<CourseReadDto> CreateCourseAsync(CourseCreateDto courseDto)
        {
            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                Credits = courseDto.Credits,
                InstructorId = courseDto.InstructorId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return await GetCourseByIdAsync(course.Id) ?? throw new Exception("Course not found after creation.");
        }

        /// <summary>Updates an existing course. Returns false if not found.</summary>
        public async Task<bool> UpdateCourseAsync(int id, CourseUpdateDto courseDto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            course.Title = courseDto.Title ?? course.Title;
            course.Description = courseDto.Description ?? course.Description;
            if (courseDto.Credits != 0)
                course.Credits = courseDto.Credits;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>Soft-deletes a course by setting IsDeleted = true. Returns false if not found.</summary>
        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            course.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>Hard-deletes a course (Admin only). Bypasses soft-delete filter via IgnoreQueryFilters.</summary>
        public async Task<bool> HardDeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
