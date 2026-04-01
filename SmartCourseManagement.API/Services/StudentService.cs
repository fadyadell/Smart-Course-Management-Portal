using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles student listing and lookup. Filters users by role "Student".
    /// Uses AsNoTracking() + Select() LINQ projections for optimization.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all users with the Student role, projected to StudentReadDto.</summary>
        public async Task<IEnumerable<StudentReadDto>> GetAllStudentsAsync()
        {
            return await _context.Users
                .AsNoTracking() // Read-only optimization
                .Where(u => u.Role == "Student")
                .Select(u => new StudentReadDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToListAsync();
        }

        /// <summary>Returns a single student by ID.</summary>
        public async Task<StudentReadDto> GetStudentByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id && u.Role == "Student")
                .Select(u => new StudentReadDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();
        }
    }
}
