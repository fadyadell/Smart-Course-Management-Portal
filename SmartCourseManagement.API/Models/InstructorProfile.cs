using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// One-to-One with User. An instructor profile holds extra info about a user with the Instructor role.
    /// One-to-Many with Course (an instructor teaches many courses).
    /// Inherits audit and soft-delete fields from BaseEntity.
    /// </summary>
    public class InstructorProfile : BaseEntity
    {

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
