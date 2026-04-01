using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;
using SmartCourseManagement.API.Services;
using Xunit;

namespace SmartCourseManagement.Tests.Services
{
    /// <summary>
    /// Unit tests for CourseService: CRUD operations, pagination, filtering, and soft delete.
    /// </summary>
    public class CourseServiceTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private async Task SeedAsync(AppDbContext ctx)
        {
            // Seed a user and instructor profile
            var user = new User { Name = "Dr. Test", Email = "dr@test.com", PasswordHash = "x", Role = "Instructor" };
            ctx.Users.Add(user);
            var profile = new InstructorProfile { User = user, Biography = "Bio", OfficeLocation = "Room 1" };
            ctx.InstructorProfiles.Add(profile);
            await ctx.SaveChangesAsync();

            // Seed 3 courses
            ctx.Courses.AddRange(
                new Course { Title = "Algebra",   Description = "Math", Credits = 3, InstructorId = profile.Id },
                new Course { Title = "Biology",   Description = "Science", Credits = 4, InstructorId = profile.Id },
                new Course { Title = "Chemistry", Description = "Lab science", Credits = 3, InstructorId = profile.Id }
            );
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetCourses_NoPaging_ReturnsAll()
        {
            using var ctx = CreateContext(nameof(GetCourses_NoPaging_ReturnsAll));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var result = await service.GetCoursesAsync(new CourseQueryParams { Page = 1, PageSize = 10 });

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count());
        }

        [Fact]
        public async Task GetCourses_WithSearch_FiltersResults()
        {
            using var ctx = CreateContext(nameof(GetCourses_WithSearch_FiltersResults));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var result = await service.GetCoursesAsync(new CourseQueryParams { Search = "bio", Page = 1, PageSize = 10 });

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Biology", result.Items.First().Title);
        }

        [Fact]
        public async Task GetCourses_FilterByCredits_ReturnsMatchingCourses()
        {
            using var ctx = CreateContext(nameof(GetCourses_FilterByCredits_ReturnsMatchingCourses));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var result = await service.GetCoursesAsync(new CourseQueryParams { Credits = 3, Page = 1, PageSize = 10 });

            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetCourses_Pagination_ReturnsCorrectPage()
        {
            using var ctx = CreateContext(nameof(GetCourses_Pagination_ReturnsCorrectPage));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var result = await service.GetCoursesAsync(new CourseQueryParams { Page = 1, PageSize = 2 });

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalPages);
            Assert.True(result.HasNextPage);
        }

        [Fact]
        public async Task GetCourseById_ExistingId_ReturnsCourse()
        {
            using var ctx = CreateContext(nameof(GetCourseById_ExistingId_ReturnsCourse));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);
            var allCourses = await service.GetCoursesAsync(new CourseQueryParams { Page = 1, PageSize = 10 });
            var firstId = allCourses.Items.First().Id;

            var course = await service.GetCourseByIdAsync(firstId);

            Assert.NotNull(course);
            Assert.Equal(firstId, course!.Id);
        }

        [Fact]
        public async Task GetCourseById_NonExistingId_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(GetCourseById_NonExistingId_ReturnsNull));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var course = await service.GetCourseByIdAsync(9999);

            Assert.Null(course);
        }

        [Fact]
        public async Task CreateCourse_ValidData_ReturnsCourseDto()
        {
            using var ctx = CreateContext(nameof(CreateCourse_ValidData_ReturnsCourseDto));
            await SeedAsync(ctx);
            var instructorId = (await ctx.InstructorProfiles.FirstAsync()).Id;
            var service = new CourseService(ctx);

            var dto = new CourseCreateDto { Title = "Physics", Description = "Forces", Credits = 4, InstructorId = instructorId };
            var result = await service.CreateCourseAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Physics", result.Title);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task UpdateCourse_ValidId_UpdatesFields()
        {
            using var ctx = CreateContext(nameof(UpdateCourse_ValidId_UpdatesFields));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);
            var allCourses = await service.GetCoursesAsync(new CourseQueryParams { Page = 1, PageSize = 10 });
            var id = allCourses.Items.First().Id;

            var updated = await service.UpdateCourseAsync(id, new CourseUpdateDto { Title = "Updated Title" });
            var course = await service.GetCourseByIdAsync(id);

            Assert.True(updated);
            Assert.Equal("Updated Title", course!.Title);
        }

        [Fact]
        public async Task UpdateCourse_InvalidId_ReturnsFalse()
        {
            using var ctx = CreateContext(nameof(UpdateCourse_InvalidId_ReturnsFalse));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var result = await service.UpdateCourseAsync(9999, new CourseUpdateDto { Title = "X" });

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCourse_SoftDeletesRecord()
        {
            using var ctx = CreateContext(nameof(DeleteCourse_SoftDeletesRecord));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);
            var allCourses = await service.GetCoursesAsync(new CourseQueryParams { Page = 1, PageSize = 10 });
            var id = allCourses.Items.First().Id;

            var deleted = await service.DeleteCourseAsync(id);

            Assert.True(deleted);
            // Should not appear in normal queries (soft-delete filter active)
            var afterDelete = await service.GetCourseByIdAsync(id);
            Assert.Null(afterDelete);
            // But the record still exists in the database
            var stillInDb = await ctx.Courses.IgnoreQueryFilters().AnyAsync(c => c.Id == id && c.IsDeleted);
            Assert.True(stillInDb);
        }

        [Fact]
        public async Task DeleteCourse_InvalidId_ReturnsFalse()
        {
            using var ctx = CreateContext(nameof(DeleteCourse_InvalidId_ReturnsFalse));
            await SeedAsync(ctx);
            var service = new CourseService(ctx);

            var result = await service.DeleteCourseAsync(9999);

            Assert.False(result);
        }
    }
}
