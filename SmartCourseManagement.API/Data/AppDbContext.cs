using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Data
{
    /// <summary>
    /// Entity Framework Core DbContext. Configures all entity relationships using Fluent API.
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

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // SEED DATA
            // Note: Password is 'InstructorPass123!' hashed with BCrypt
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Name = "Dr. Jane Smith", 
                    Email = "instructor@example.com", 
                    PasswordHash = "$2a$11$XdmZbeEuxVdBoHMI/IqjpO3BhIDs6wgq8iXmVDmRS8RD4Ih3fiNrm", // "InstructorPass123!"
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
    }
}
