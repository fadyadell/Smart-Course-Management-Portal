using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCourseManagement.API.DTOs
{
    public class EnrollmentCreateDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
    }

    public class EnrollmentReadDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
