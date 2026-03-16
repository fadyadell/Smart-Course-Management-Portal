using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCourseManagement.API.DTOs
{
    public class InstructorReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Biography { get; set; }
        public string OfficeLocation { get; set; }
    }

    public class InstructorProfileUpdateDto
    {
        public string Biography { get; set; }
        public string OfficeLocation { get; set; }
    }

    public class StudentReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
