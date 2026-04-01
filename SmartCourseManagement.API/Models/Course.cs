using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Represents a course. One-to-Many with InstructorProfile (instructor teaches many courses).
    /// Many-to-Many with User(Student) via the Enrollment junction entity.
    /// </summary>
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Range(1, 10)]
        public int Credits { get; set; }

        // FK to InstructorProfile
        [Required]
        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public InstructorProfile Instructor { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        // Many-to-Many (via Enrollment junction): Many students enroll in this course
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
