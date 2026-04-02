using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCourseManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class CorrectSeedHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$XdmZbeEuxVdBoHMI/IqjpO3BhIDs6wgq8iXmVDmRS8RD4Ih3fiNrm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$qR7X8UjQ9t6W... (hashed)");
        }
    }
}
