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
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name, // LINQ projection, no extra load
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    UpdatedAt = c.UpdatedAt,
                    UpdatedBy = c.UpdatedBy,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();
        }

        /// <summary>Returns one course by ID using FirstOrDefaultAsync() + AsNoTracking().</summary>
        public async Task<CourseReadDto> GetCourseByIdAsync(int id)
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
                    CreatedBy = c.CreatedBy,
                    UpdatedAt = c.UpdatedAt,
                    UpdatedBy = c.UpdatedBy,
                    IsDeleted = c.IsDeleted
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>Returns paginated and filtered courses with search support.</summary>
        public async Task<PaginationResponseDto<CourseReadDto>> GetCoursesPagedAsync(PaginationRequestDto paginationDto)
        {
            var query = _context.Courses.AsNoTracking();

            // Apply search filter on title or description
            if (!string.IsNullOrEmpty(paginationDto.SearchTerm))
            {
                var search = paginationDto.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.Title.ToLower().Contains(search) || 
                    c.Description.ToLower().Contains(search));
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Apply sorting
            query = paginationDto.SortDescending
                ? query.OrderByDescending(c => EF.Property<object>(c, paginationDto.SortBy))
                : query.OrderBy(c => EF.Property<object>(c, paginationDto.SortBy));

            // Apply pagination
            var items = await query
                .Skip((paginationDto.Page - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.User.Name,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    UpdatedAt = c.UpdatedAt,
                    UpdatedBy = c.UpdatedBy,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            var totalPages = (totalItems + paginationDto.PageSize - 1) / paginationDto.PageSize;

            return new PaginationResponseDto<CourseReadDto>
            {
                Items = items,
                CurrentPage = paginationDto.Page,
                PageSize = paginationDto.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
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
            if (course == null) return false;

            // Only update provided fields
            course.Title = courseDto.Title ?? course.Title;
            course.Description = courseDto.Description ?? course.Description;
            if (courseDto.Credits != 0)
                course.Credits = courseDto.Credits;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>Deletes a course by ID. Returns false if not found.</summary>
        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
