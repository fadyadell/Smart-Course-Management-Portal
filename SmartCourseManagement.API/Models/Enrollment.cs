using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Junction entity that implements the Many-to-Many relationship between Students (User) and Courses.
    /// A student can enroll in many courses; a course can have many students.
    /// </summary>
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // FK to the student (User)
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public User Student { get; set; }

        // FK to the course
        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        /// <summary>Date when the student enrolled (set automatically to UTC now)</summary>
        [Required]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }
}
