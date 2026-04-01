using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.DTOs
{
    // DTO for creating a new course
    public class CourseCreateDto
    {
        [Required(ErrorMessage = "Course title is required")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credits are required")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "InstructorId is required")]
        public int InstructorId { get; set; }
    }

    // DTO for updating an existing course
    public class CourseUpdateDto
    {
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int Credits { get; set; }
    }

    // DTO returned when reading course data
    public class CourseReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Paginated result wrapper
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    // Query parameters for filtering / searching courses
    public class CourseQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? InstructorId { get; set; }
        public int? Credits { get; set; }
        public string? SortBy { get; set; } = "title";
        public bool SortDesc { get; set; } = false;
    }
}
