using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.DTOs
{
    // DTO returned when reading instructor profile data
    public class InstructorReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Biography { get; set; }
        public string OfficeLocation { get; set; }
    }

    // DTO for updating instructor profile
    public class InstructorProfileUpdateDto
    {
        [MaxLength(500, ErrorMessage = "Biography cannot exceed 500 characters")]
        public string Biography { get; set; }

        [MaxLength(100, ErrorMessage = "Office location cannot exceed 100 characters")]
        public string OfficeLocation { get; set; }
    }
}
