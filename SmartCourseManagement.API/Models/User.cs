using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Represents a user in the system (Admin, Instructor, or Student).
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Possible values: Admin, Instructor, Student</summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        // Audit fields – auto-set by AppDbContext.SaveChanges
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // One-to-One: User has one InstructorProfile (nullable for non-instructors)
        public InstructorProfile? InstructorProfile { get; set; }

        // One-to-Many (via junction): User(Student) has many Enrollments
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        // One-to-Many: User has many refresh tokens
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
