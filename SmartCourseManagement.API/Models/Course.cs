using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
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

        [Required]
        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public SmartCourseManagement.API.Models.InstructorProfile Instructor { get; set; }

        // Navigation Properties
        public ICollection<SmartCourseManagement.API.Models.Enrollment> Enrollments { get; set; } = new List<SmartCourseManagement.API.Models.Enrollment>();
    }
}
