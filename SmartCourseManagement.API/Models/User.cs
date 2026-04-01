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
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        /// <summary>Possible values: Admin, Instructor, Student</summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        // One-to-One: User has one InstructorProfile (nullable for non-instructors)
        public InstructorProfile InstructorProfile { get; set; }

        // One-to-Many (via junction): User(Student) has many Enrollments
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
