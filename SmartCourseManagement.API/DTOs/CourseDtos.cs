using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCourseManagement.API.DTOs
{
    public class CourseCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public int InstructorId { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
    }

    public class CourseReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; }
    }
}
