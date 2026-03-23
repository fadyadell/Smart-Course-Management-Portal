using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartCourseManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class FinalSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash", "Role" },
                values: new object[] { 1, "instructor@example.com", "Dr. Jane Smith", "$2a$11$qR7X8UjQ9t6W... (hashed)", "Instructor" });

            migrationBuilder.InsertData(
                table: "InstructorProfiles",
                columns: new[] { "Id", "Biography", "OfficeLocation", "UserId" },
                values: new object[] { 1, "Professor of Computer Science with 15 years experience.", "Science Building, Lab 404", 1 });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Credits", "Description", "InstructorId", "Title" },
                values: new object[,]
                {
                    { 1, 3, "Learn the basics of building high-performance Web APIs.", 1, "Introduction to ASP.NET Core" },
                    { 2, 4, "Master complex relationships and performance tuning.", 1, "Advanced Entity Framework Core" },
                    { 3, 3, "Build responsive and vibrant SPAs without heavy frameworks.", 1, "Frontend Mastery with Vanilla JS" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InstructorProfiles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
