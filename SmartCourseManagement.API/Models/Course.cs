using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Represents a course. One-to-Many with InstructorProfile (instructor teaches many courses).
    /// Many-to-Many with User(Student) via the Enrollment junction entity.
    /// Inherits audit and soft-delete fields from BaseEntity.
    /// </summary>
    public class Course : BaseEntity
    {

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Credits { get; set; }

        // FK to InstructorProfile
        [Required]
        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public InstructorProfile Instructor { get; set; } = null!;

        // Soft delete – records are never physically removed, just hidden
        public bool IsDeleted { get; set; } = false;

        // Audit fields – auto-set by AppDbContext.SaveChanges
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Many-to-Many (via Enrollment junction): Many students enroll in this course
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
