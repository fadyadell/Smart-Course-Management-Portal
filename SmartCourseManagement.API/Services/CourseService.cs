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
    /// Handles course CRUD with pagination, search/filter, and soft delete.
    /// The global query filter on Course (IsDeleted == false) is applied automatically.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;

        public CourseService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a paginated, filtered, and sorted list of courses.
        /// Soft-deleted courses are excluded by the EF Core global query filter.
        /// </summary>
        public async Task<PagedResult<CourseReadDto>> GetCoursesAsync(CourseQueryParams q)
        {
            var query = _context.Courses
                .AsNoTracking()
                .AsQueryable();

            // Filter by search term (title or description)
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var search = q.Search.Trim().ToLower();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(search) ||
                    c.Description.ToLower().Contains(search) ||
                    c.Instructor.User.Name.ToLower().Contains(search));
            }

            // Filter by instructor
            if (q.InstructorId.HasValue)
                query = query.Where(c => c.InstructorId == q.InstructorId.Value);

            // Filter by credits
            if (q.Credits.HasValue)
                query = query.Where(c => c.Credits == q.Credits.Value);

            // Sorting
            query = (q.SortBy?.ToLower(), q.SortDesc) switch
            {
                ("credits", false) => query.OrderBy(c => c.Credits),
                ("credits", true) => query.OrderByDescending(c => c.Credits),
                ("createdat", false) => query.OrderBy(c => c.CreatedAt),
                ("createdat", true) => query.OrderByDescending(c => c.CreatedAt),
                (_, false) => query.OrderBy(c => c.Title),
                (_, true) => query.OrderByDescending(c => c.Title)
            };

            var totalCount = await query.CountAsync();
            var page = Math.Max(1, q.Page);
            var pageSize = q.PageSize is > 0 and <= 100 ? q.PageSize : 10;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            return new PagedResult<CourseReadDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>Returns one course by ID. Returns null if not found (respects soft-delete filter).</summary>
        public async Task<CourseReadDto?> GetCourseByIdAsync(int id)
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

            return await GetCourseByIdAsync(course.Id) ?? throw new Exception("Failed to retrieve created course.");
        }

        /// <summary>Updates an existing course. Returns false if not found.</summary>
        public async Task<bool> UpdateCourseAsync(int id, CourseUpdateDto courseDto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.IsDeleted) return false;

            if (courseDto.Title != null) course.Title = courseDto.Title;
            if (courseDto.Description != null) course.Description = courseDto.Description;
            if (courseDto.Credits > 0) course.Credits = courseDto.Credits;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Soft-deletes a course (sets IsDeleted = true).
        /// The record remains in the database for history/audit purposes.
        /// Returns false if not found.
        /// </summary>
        public async Task<bool> DeleteCourseAsync(int id)
        {
            // IgnoreQueryFilters lets us find already-soft-deleted records too
            var course = await _context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return false;

            course.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
