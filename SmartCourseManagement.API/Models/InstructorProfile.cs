using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// One-to-One with User. An instructor profile holds extra info about a user with the Instructor role.
    /// One-to-Many with Course (an instructor teaches many courses).
    /// </summary>
    public class InstructorProfile
    {
        [Key]
        public int Id { get; set; }

        // One-to-One FK to User
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [MaxLength(500)]
        public string Biography { get; set; }

        [MaxLength(100)]
        public string OfficeLocation { get; set; }

        // One-to-Many: An instructor teaches many courses
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
