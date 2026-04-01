using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Data
{
    /// <summary>
    /// Entity Framework Core DbContext. Configures all entity relationships using Fluent API.
    /// Includes: soft-delete global filter on Course, audit field auto-update, refresh tokens.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<InstructorProfile> InstructorProfiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Automatically updates audit timestamps before every save.
        /// </summary>
        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var now = DateTime.UtcNow;
            foreach (EntityEntry entry in ChangeTracker.Entries())
            {
                if (entry.Entity is User u)
                {
                    if (entry.State == EntityState.Added) u.CreatedAt = now;
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified) u.UpdatedAt = now;
                }
                if (entry.Entity is Course c)
                {
                    if (entry.State == EntityState.Added) c.CreatedAt = now;
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified) c.UpdatedAt = now;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // SOFT DELETE GLOBAL FILTER — Course
            // All LINQ queries on Courses automatically exclude soft-deleted records
            // =============================================
            modelBuilder.Entity<Course>()
                .HasQueryFilter(c => !c.IsDeleted);

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

            // =============================================
            // ONE-TO-MANY: User -> RefreshTokens
            // =============================================
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // SEED DATA
            // Note: Password is 'Password123' hashed with BCrypt
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Dr. Jane Smith",
                    Email = "instructor@example.com",
                    PasswordHash = "$2a$11$S8mJpx/o7u6H1iU96J10nuvL1gYhX6A9N5X/B8p3bY7fF.E2f/v1i",
                    Role = "Instructor",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
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
                new Course { Id = 1, Title = "Introduction to ASP.NET Core", Description = "Learn the basics of building high-performance Web APIs.", Credits = 3, InstructorId = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
                new Course { Id = 2, Title = "Advanced Entity Framework Core", Description = "Master complex relationships and performance tuning.", Credits = 4, InstructorId = 1, CreatedAt = seedDate, UpdatedAt = seedDate },
                new Course { Id = 3, Title = "Frontend Mastery with Vanilla JS", Description = "Build responsive and vibrant SPAs without heavy frameworks.", Credits = 3, InstructorId = 1, CreatedAt = seedDate, UpdatedAt = seedDate }
            );
        }
    }
}
