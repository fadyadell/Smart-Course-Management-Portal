using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Handles instructor profile read and update operations.
    /// Demonstrates One-to-One relationship navigation (InstructorProfile -> User).
    /// </summary>
    public class InstructorService : IInstructorService
    {
        private readonly AppDbContext _context;

        public InstructorService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all instructor profiles projected to InstructorReadDto using AsNoTracking() + Select().</summary>
        public async Task<IEnumerable<InstructorReadDto>> GetAllInstructorsAsync()
        {
            return await _context.InstructorProfiles
                .AsNoTracking()
                .Select(p => new InstructorReadDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.Name, // Navigate One-to-One to get user name
                    Biography = p.Biography,
                    OfficeLocation = p.OfficeLocation
                })
                .ToListAsync();
        }

        /// <summary>Returns a single instructor profile by profile ID.</summary>
        public async Task<InstructorReadDto> GetInstructorByIdAsync(int id)
        {
            return await _context.InstructorProfiles
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new InstructorReadDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    Biography = p.Biography,
                    OfficeLocation = p.OfficeLocation
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>Updates an instructor's biography and office location by their User ID.</summary>
        public async Task<bool> UpdateInstructorProfileAsync(int userId, InstructorProfileUpdateDto profileDto)
        {
            var profile = await _context.InstructorProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return false;

            // Patch: only update provided fields
            profile.Biography = profileDto.Biography ?? profile.Biography;
            profile.OfficeLocation = profileDto.OfficeLocation ?? profile.OfficeLocation;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
