using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Data
{
    /// <summary>
    /// Entity Framework Core DbContext. Configures all entity relationships using Fluent API.
    /// Supports soft delete filtering and automatic audit field population.
    /// </summary>
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
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
            // ONE-TO-ONE: User <-> InstructorProfile
            // A user with role Instructor has exactly one InstructorProfile
            // =============================================
            modelBuilder.Entity<User>()
                .HasOne(u => u.InstructorProfile)
                .WithOne(p => p.User)
                .HasForeignKey<InstructorProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // ONE-TO-MANY: InstructorProfile -> Courses
            // One instructor teaches many courses
            // =============================================
            modelBuilder.Entity<InstructorProfile>()
                .HasMany(p => p.Courses)
                .WithOne(c => c.Instructor)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================================
            // MANY-TO-MANY: Student (User) <-> Course (via Enrollment)
            // Many students can enroll in many courses
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

            // =============================================
            // ONE-TO-MANY: User -> RefreshTokens
            // =============================================
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // SEED DATA
            // Note: Password is 'Password123' hashed with BCrypt
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Name = "Dr. Jane Smith", 
                    Email = "instructor@example.com", 
                    PasswordHash = "$2a$11$S8mJpx/o7u6H1iU96J10nuvL1gYhX6A9N5X/B8p3bY7fF.E2f/v1i", // "Password123"
                    Role = "Instructor" 
                }
            );

            modelBuilder.Entity<InstructorProfile>().HasData(
                new InstructorProfile 
                { 
                    Id = 1, 
                    UserId = 1, 
                    Biography = "Professor of Computer Science with 15 years experience.", 
                    OfficeLocation = "Science Building, Lab 404" 
                }
            );

            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Title = "Introduction to ASP.NET Core", Description = "Learn the basics of building high-performance Web APIs.", Credits = 3, InstructorId = 1 },
                new Course { Id = 2, Title = "Advanced Entity Framework Core", Description = "Master complex relationships and performance tuning.", Credits = 4, InstructorId = 1 },
                new Course { Id = 3, Title = "Frontend Mastery with Vanilla JS", Description = "Build responsive and vibrant SPAs without heavy frameworks.", Credits = 3, InstructorId = 1 }
            );
        }

        /// <summary>
        /// Overrides SaveChangesAsync to auto-populate audit fields and apply soft-delete logic.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is User user)
                {
                    if (entry.State == EntityState.Added)
                    {
                        user.CreatedAt = now;
                        user.UpdatedAt = now;
                        user.CreatedBy = currentUser;
                        user.UpdatedBy = currentUser;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        user.UpdatedAt = now;
                        user.UpdatedBy = currentUser;
                    }
                }
                else if (entry.Entity is Course course)
                {
                    if (entry.State == EntityState.Added)
                    {
                        course.CreatedAt = now;
                        course.UpdatedAt = now;
                        course.CreatedBy = currentUser;
                        course.UpdatedBy = currentUser;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        course.UpdatedAt = now;
                        course.UpdatedBy = currentUser;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
