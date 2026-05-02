using System;
using System.Collections.Generic;

namespace SmartCourseManagement.API.DTOs
{
    /// <summary>Base DTO for all paginated GET requests.</summary>
    public class PagedRequest
    {
        /// <summary>1-based page number. Defaults to 1.</summary>
        public int Page { get; set; } = 1;

        /// <summary>Number of items per page. Defaults to 10, max 100.</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>Optional free-text search term applied to relevant string fields.</summary>
        public string? SearchTerm { get; set; }

        /// <summary>Optional sort field name (e.g. "name", "credits").</summary>
        public string? SortBy { get; set; }

        /// <summary>Sort direction: "asc" (default) or "desc".</summary>
        public string SortDirection { get; set; } = "asc";

        // Helpers
        public int Skip => (Page - 1) * PageSize;
        public int Take => Math.Min(PageSize, 100);
    }

    /// <summary>Request DTO for filtering courses.</summary>
    public class CourseFilterRequest : PagedRequest
    {
        /// <summary>Filter by instructor name (partial match, case-insensitive).</summary>
        public string? InstructorName { get; set; }

        /// <summary>Filter by minimum credits.</summary>
        public int? MinCredits { get; set; }

        /// <summary>Filter by maximum credits.</summary>
        public int? MaxCredits { get; set; }
    }

    /// <summary>Request DTO for filtering enrollments.</summary>
    public class EnrollmentFilterRequest : PagedRequest
    {
        /// <summary>Filter by student name or email (partial match).</summary>
        public string? StudentSearch { get; set; }

        /// <summary>Filter by course title (partial match).</summary>
        public string? CourseSearch { get; set; }
    }

    /// <summary>Generic paginated response wrapper.</summary>
    public class PagedResponse<T>
    {
        /// <summary>The items in this page.</summary>
        public IEnumerable<T> Data { get; set; } = new List<T>();

        /// <summary>Total number of records matching the filter (before paging).</summary>
        public int Total { get; set; }

        /// <summary>Current page number (1-based).</summary>
        public int Page { get; set; }

        /// <summary>Number of items per page.</summary>
        public int PageSize { get; set; }

        /// <summary>Total number of pages available.</summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

        public PagedResponse() { }

        public PagedResponse(IEnumerable<T> data, int total, int page, int pageSize)
        {
            Data = data;
            Total = total;
            Page = page;
            PageSize = pageSize;
        }
    }

    /// <summary>Standardized wrapper for all API responses.</summary>
    public class ApiResponse<T>
    {
        /// <summary>Whether the operation succeeded.</summary>
        public bool Success { get; set; }

        /// <summary>HTTP status code.</summary>
        public int StatusCode { get; set; }

        /// <summary>Human-readable message.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Response payload (null on error).</summary>
        public T? Data { get; set; }

        /// <summary>List of error details (populated on failure).</summary>
        public IEnumerable<string> Errors { get; set; } = new List<string>();

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new ApiResponse<T> { Success = true, StatusCode = 200, Message = message, Data = data };

        public static ApiResponse<T> Fail(int statusCode, string message, IEnumerable<string>? errors = null) =>
            new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors ?? Array.Empty<string>()
            };
    }
}
