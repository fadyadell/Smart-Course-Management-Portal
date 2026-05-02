using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.DTOs
{
    /// <summary>
    /// Common request parameters for paginated and filtered list endpoints.
    /// </summary>
    public class PagedRequest
    {
        private int _page = 1;
        private int _pageSize = 10;

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
        }

        public string SearchTerm { get; set; } = string.Empty;

        public string SortBy { get; set; } = string.Empty;

        public string Filter { get; set; } = string.Empty;
    }
}
