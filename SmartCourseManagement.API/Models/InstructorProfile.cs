using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    public class InstructorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public SmartCourseManagement.API.Models.User User { get; set; }

        [MaxLength(500)]
        public string Biography { get; set; }

        [MaxLength(100)]
        public string OfficeLocation { get; set; }

        // Navigation Properties
        public ICollection<SmartCourseManagement.API.Models.Course> Courses { get; set; } = new List<SmartCourseManagement.API.Models.Course>();
    }
}
