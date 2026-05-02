using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles student listing and lookup with pagination. Filters users by role "Student".
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns paginated students, optionally filtered by SearchTerm (name or email).</summary>
        public async Task<PagedResponse<StudentReadDto>> GetAllStudentsAsync(PagedRequest request)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "Student");

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("email", "desc") => query.OrderByDescending(u => u.Email),
                ("email", _) => query.OrderBy(u => u.Email),
                (_, "desc") => query.OrderByDescending(u => u.Name),
                _ => query.OrderBy(u => u.Name)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(u => new StudentReadDto { Id = u.Id, Name = u.Name, Email = u.Email })
                .ToListAsync();

            return new PagedResponse<StudentReadDto>(items, total, request.Page, request.PageSize);
        }

        /// <summary>Returns a single student by ID.</summary>
        public async Task<StudentReadDto?> GetStudentByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id && u.Role == "Student")
                .Select(u => new StudentReadDto { Id = u.Id, Name = u.Name, Email = u.Email })
                .FirstOrDefaultAsync();
        }
    }
}
