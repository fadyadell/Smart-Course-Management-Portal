using System;
using System.Collections.Generic;

namespace SmartCourseManagement.API.DTOs
{
    /// <summary>
    /// Generic wrapper for paginated responses.
    /// </summary>
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

        public PagedResponse(IEnumerable<T> data, int total, int page, int pageSize)
        {
            Data = data;
            Total = total;
            Page = page;
            PageSize = pageSize;
        }
    }
}
