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
    /// Handles all course CRUD operations. Uses AsNoTracking() for reads
    /// and Select() projections to return DTOs (not entities).
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;

        public CourseService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all courses projected to CourseReadDto using Select() + AsNoTracking().</summary>
        public async Task<IEnumerable<CourseReadDto>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .AsNoTracking() // Read-only: no change tracking needed
                .Where(c => !c.IsDeleted)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name // LINQ projection, no extra load
                })
                .ToListAsync();
        }

        /// <summary>
        /// Returns a paginated, filtered, and sorted list of courses.
        /// Supports search by title/description and sorting by title, credits, or instructor.
        /// </summary>
        public async Task<PagedResponse<CourseReadDto>> GetCoursesAsync(PagedRequest request)
        {
            var query = _context.Courses
                .AsNoTracking()
                .Where(c => !c.IsDeleted);

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(term) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)));
            }

            // Additional filter by credits
            if (!string.IsNullOrWhiteSpace(request.Filter) && int.TryParse(request.Filter, out var credits))
            {
                query = query.Where(c => c.Credits == credits);
            }

            // Sorting
            query = request.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(c => c.Title),
                "credits" => query.OrderBy(c => c.Credits),
                "credits_desc" => query.OrderByDescending(c => c.Credits),
                _ => query.OrderBy(c => c.Id)
            };

            var total = await query.CountAsync();

            var data = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name
                })
                .ToListAsync();

            return new PagedResponse<CourseReadDto>(data, total, request.Page, request.PageSize);
        }

        /// <summary>Returns one course by ID using FirstOrDefaultAsync() + AsNoTracking().</summary>
        public async Task<CourseReadDto> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .AsNoTracking()
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name
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

            // Fetch the full projection after save
            return await GetCourseByIdAsync(course.Id);
        }

        /// <summary>Updates an existing course. Returns false if not found.</summary>
        public async Task<bool> UpdateCourseAsync(int id, CourseUpdateDto courseDto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.IsDeleted) return false;

            // Only update provided fields
            course.Title = courseDto.Title ?? course.Title;
            course.Description = courseDto.Description ?? course.Description;
            if (courseDto.Credits != 0)
                course.Credits = courseDto.Credits;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>Soft-deletes a course by ID. Returns false if not found.</summary>
        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.IsDeleted) return false;

            // Soft delete: mark as deleted instead of removing from database
            course.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
