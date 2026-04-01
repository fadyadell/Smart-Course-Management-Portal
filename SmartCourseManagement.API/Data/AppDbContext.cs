using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Data
{
    /// <summary>
    /// Entity Framework Core DbContext. Configures all entity relationships using Fluent API.
    /// Overrides SaveChangesAsync to auto-set audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
    /// and applies global soft-delete query filters so deleted records are never returned by default.
    /// </summary>
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<InstructorProfile> InstructorProfiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // SOFT DELETE GLOBAL QUERY FILTERS
            // Automatically exclude soft-deleted records from all queries
            // =============================================
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<InstructorProfile>().HasQueryFilter(p => !p.IsDeleted);

            // =============================================
            // ONE-TO-ONE: User <-> InstructorProfile
            // =============================================
            modelBuilder.Entity<User>()
                .HasOne(u => u.InstructorProfile)
                .WithOne(p => p.User)
                .HasForeignKey<InstructorProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // ONE-TO-MANY: InstructorProfile -> Courses
            // =============================================
            modelBuilder.Entity<InstructorProfile>()
                .HasMany(p => p.Courses)
                .WithOne(c => c.Instructor)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================================
            // MANY-TO-MANY: Student (User) <-> Course (via Enrollment)
            // =============================================
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ONE-TO-MANY: User -> RefreshTokens
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // =============================================
            // SEED DATA - audit fields use a fixed UTC date for deterministic migrations
            // =============================================
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Dr. Jane Smith",
                    Email = "instructor@example.com",
                    PasswordHash = "$2a$11$S8mJpx/o7u6H1iU96J10nuvL1gYhX6A9N5X/B8p3bY7fF.E2f/v1i",
                    Role = "Instructor",
                    IsDeleted = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "system",
                    UpdatedBy = "system"
                }
            );

            modelBuilder.Entity<InstructorProfile>().HasData(
                new InstructorProfile
                {
                    Id = 1,
                    UserId = 1,
                    Biography = "Professor of Computer Science with 15 years experience.",
                    OfficeLocation = "Science Building, Lab 404",
                    IsDeleted = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "system",
                    UpdatedBy = "system"
                }
            );

            modelBuilder.Entity<Course>().HasData(
                new Course
                {
                    Id = 1, Title = "Introduction to ASP.NET Core",
                    Description = "Learn the basics of building high-performance Web APIs.",
                    Credits = 3, InstructorId = 1,
                    IsDeleted = false, CreatedAt = seedDate, UpdatedAt = seedDate,
                    CreatedBy = "system", UpdatedBy = "system"
                },
                new Course
                {
                    Id = 2, Title = "Advanced Entity Framework Core",
                    Description = "Master complex relationships and performance tuning.",
                    Credits = 4, InstructorId = 1,
                    IsDeleted = false, CreatedAt = seedDate, UpdatedAt = seedDate,
                    CreatedBy = "system", UpdatedBy = "system"
                },
                new Course
                {
                    Id = 3, Title = "Frontend Mastery with Vanilla JS",
                    Description = "Build responsive and vibrant SPAs without heavy frameworks.",
                    Credits = 3, InstructorId = 1,
                    IsDeleted = false, CreatedAt = seedDate, UpdatedAt = seedDate,
                    CreatedBy = "system", UpdatedBy = "system"
                }
            );
        }

        /// <summary>
        /// Overrides SaveChangesAsync to automatically populate audit fields.
        /// - CreatedAt / CreatedBy set only on first add.
        /// - UpdatedAt / UpdatedBy refreshed on every modification.
        /// - IsDeleted set to false on creation so new records are always visible.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentUserEmail = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value ?? "system";

            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedBy = currentUserEmail;
                    entry.Entity.UpdatedBy = currentUserEmail;
                    entry.Entity.IsDeleted = false;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserEmail;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
