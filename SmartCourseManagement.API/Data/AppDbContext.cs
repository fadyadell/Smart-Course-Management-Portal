using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SmartCourseManagement.API.Models.User> Users { get; set; }
        public DbSet<SmartCourseManagement.API.Models.InstructorProfile> InstructorProfiles { get; set; }
        public DbSet<SmartCourseManagement.API.Models.Course> Courses { get; set; }
        public DbSet<SmartCourseManagement.API.Models.Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One-to-One: User -> InstructorProfile
            modelBuilder.Entity<SmartCourseManagement.API.Models.User>()
                .HasOne(u => u.InstructorProfile)
                .WithOne(p => p.User)
                .HasForeignKey<SmartCourseManagement.API.Models.InstructorProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: InstructorProfile -> Courses
            modelBuilder.Entity<SmartCourseManagement.API.Models.InstructorProfile>()
                .HasMany(p => p.Courses)
                .WithOne(c => c.Instructor)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-Many: Students (User) <-> Courses via Enrollment
            modelBuilder.Entity<SmartCourseManagement.API.Models.Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SmartCourseManagement.API.Models.Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
