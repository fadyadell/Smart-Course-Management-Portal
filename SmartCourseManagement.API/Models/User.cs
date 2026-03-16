using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.Models
{
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

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // Admin, Instructor, Student

        // Navigation Properties
        public SmartCourseManagement.API.Models.InstructorProfile InstructorProfile { get; set; }
        public ICollection<SmartCourseManagement.API.Models.Enrollment> Enrollments { get; set; } = new List<SmartCourseManagement.API.Models.Enrollment>();
    }
}
