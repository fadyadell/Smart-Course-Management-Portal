namespace SmartCourseManagement.API.DTOs
{
    // DTO returned when reading student data
    public class StudentReadDto
    {
        public int Id { get; set; }
        /// <summary>Alias for Id — the frontend uses "userId" for consistency across DTOs.</summary>
        public int UserId => Id;
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
