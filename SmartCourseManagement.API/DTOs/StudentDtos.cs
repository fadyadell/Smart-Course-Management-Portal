namespace SmartCourseManagement.API.DTOs
{
    // DTO returned when reading student data
    public class StudentReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
