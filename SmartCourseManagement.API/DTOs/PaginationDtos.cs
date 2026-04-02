using System.Collections.Generic;

namespace SmartCourseManagement.API.DTOs
{
    /// <summary>
    /// Common DTO for pagination requests
    /// </summary>
    public class PaginationRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public string SortBy { get; set; } = "Id";
        public bool SortDescending { get; set; } = false;
    }

    /// <summary>
    /// Generic pagination response wrapper
    /// </summary>
    public class PaginationResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
    }
}
