using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;

namespace SmartCourseManagement.API.Controllers
{
    /// <summary>
    /// Admin-only endpoint for managing users: list with pagination, view, update role, delete.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Get paginated list of all users. Admin only.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<UserReadDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
            }

            var total = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = pageSize is > 0 and <= 100 ? pageSize : 10;

            var items = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserReadDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role })
                .ToListAsync();

            return Ok(new PagedResult<UserReadDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        /// <summary>Get a specific user by ID. Admin only.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserReadDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { message = $"User {id} not found." });
            return Ok(user);
        }

        /// <summary>Update a user's name, email, or role. Admin only.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = $"User {id} not found." });

            if (dto.Email != null && dto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    return BadRequest(new { message = "Email is already in use." });
                user.Email = dto.Email;
            }

            if (dto.Name != null) user.Name = dto.Name;
            if (dto.Role != null) user.Role = dto.Role;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Delete a user permanently. Admin only.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = $"User {id} not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
