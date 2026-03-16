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
    public class InstructorService : IInstructorService
    {
        private readonly AppDbContext _context;

        public InstructorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InstructorReadDto>> GetAllInstructorsAsync()
        {
            return await _context.InstructorProfiles
                .AsNoTracking()
                .Select(p => new InstructorReadDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    Biography = p.Biography,
                    OfficeLocation = p.OfficeLocation
                })
                .ToListAsync();
        }

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

        public async Task<bool> UpdateInstructorProfileAsync(int userId, InstructorProfileUpdateDto profileDto)
        {
            var profile = await _context.InstructorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return false;

            profile.Biography = profileDto.Biography ?? profile.Biography;
            profile.OfficeLocation = profileDto.OfficeLocation ?? profile.OfficeLocation;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
