using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Data
{
    /// <summary>
    /// Entity Framework Core DbContext. Configures all entity relationships using Fluent API.
    /// Automatically handles audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy).
    /// Supports soft deletion via IsDeleted flag.
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

        /// <summary>
        /// Intercepts SaveChanges to auto-set audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy).
        /// Also marks DeletedAt and DeletedBy when soft-deleting.
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Sets CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy for BaseEntity instances.
        /// </summary>
        private void UpdateAuditFields()
        {
            var createdBy = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                var now = DateTime.UtcNow;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = createdBy;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = createdBy;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = createdBy;
                        break;

                    case EntityState.Deleted:
                        // Soft delete: instead of removing, mark as deleted
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = createdBy;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = createdBy;
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // GLOBAL QUERY FILTER: Exclude soft-deleted records from all queries
            // =============================================
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<InstructorProfile>().HasQueryFilter(ip => !ip.IsDeleted);
            modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.IsDeleted);

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
            var seedDateTime = new DateTime(2025, 3, 23, 7, 30, 0, DateTimeKind.Utc);
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Name = "Dr. Jane Smith", 
                    Email = "instructor@example.com", 
                    PasswordHash = "$2a$11$XdmZbeEuxVdBoHMI/IqjpO3BhIDs6wgq8iXmVDmRS8RD4Ih3fiNrm", // "InstructorPass123!"
                    Role = "Instructor",
                    CreatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedAt = seedDateTime,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<InstructorProfile>().HasData(
                new InstructorProfile 
                { 
                    Id = 1, 
                    UserId = 1, 
                    Biography = "Professor of Computer Science with 15 years experience.", 
                    OfficeLocation = "Science Building, Lab 404",
                    CreatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedAt = seedDateTime,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<Course>().HasData(
                new Course 
                { 
                    Id = 1, 
                    Title = "Introduction to ASP.NET Core", 
                    Description = "Learn the basics of building high-performance Web APIs.", 
                    Credits = 3, 
                    InstructorId = 1,
                    CreatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedAt = seedDateTime,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Course 
                { 
                    Id = 2, 
                    Title = "Advanced Entity Framework Core", 
                    Description = "Master complex relationships and performance tuning.", 
                    Credits = 4, 
                    InstructorId = 1,
                    CreatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedAt = seedDateTime,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Course 
                { 
                    Id = 3, 
                    Title = "Frontend Mastery with Vanilla JS", 
                    Description = "Build responsive and vibrant SPAs without heavy frameworks.", 
                    Credits = 3, 
                    InstructorId = 1,
                    CreatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedAt = seedDateTime,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

