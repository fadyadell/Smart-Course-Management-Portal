using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;
using SmartCourseManagement.API.Services;
using Xunit;

namespace SmartCourseManagement.Tests
{
    /// <summary>
    /// Unit tests for CourseService covering pagination, filtering, CRUD, and soft delete.
    /// </summary>
    public class CourseServiceTests
    {
        private static readonly DateTime SeedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>Seeds the in-memory database with an instructor, profile, and sample courses.</summary>
        private static async Task SeedAsync(AppDbContext context)
        {
            var instructor = new User
            {
                Id = 10,
                Name = "Prof. Smith",
                Email = "prof@test.com",
                PasswordHash = "hash",
                Role = "Instructor",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            };
            context.Users.Add(instructor);

            var profile = new InstructorProfile
            {
                Id = 10,
                UserId = 10,
                Biography = "Test bio",
                OfficeLocation = "Room 101",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            };
            context.InstructorProfiles.Add(profile);

            context.Courses.AddRange(
                new Course
                {
                    Id = 101, Title = "ASP.NET Core Basics", Description = "Web API intro",
                    Credits = 3, InstructorId = 10,
                    CreatedAt = SeedDate, UpdatedAt = SeedDate
                },
                new Course
                {
                    Id = 102, Title = "Entity Framework Deep Dive", Description = "ORM mastery",
                    Credits = 4, InstructorId = 10,
                    CreatedAt = SeedDate, UpdatedAt = SeedDate
                },
                new Course
                {
                    Id = 103, Title = "JavaScript Fundamentals", Description = "Frontend basics",
                    Credits = 2, InstructorId = 10,
                    CreatedAt = SeedDate, UpdatedAt = SeedDate
                }
            );

            await context.SaveChangesAsync();
        }

        // ── Test 1: GetAll returns all courses on first page ──────────────────────
        [Fact]
        public async Task GetAllCourses_NoFilter_ReturnsAllCourses()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var result = await service.GetAllCoursesAsync(new CourseFilterRequest());

            Assert.Equal(3, result.Total);
            Assert.Equal(3, result.Data.Count());
        }

        // ── Test 2: Pagination returns correct page size ──────────────────────────
        [Fact]
        public async Task GetAllCourses_WithPagination_ReturnsCorrectPage()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var result = await service.GetAllCoursesAsync(new CourseFilterRequest { Page = 1, PageSize = 2 });

            Assert.Equal(3, result.Total);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.TotalPages);
        }

        // ── Test 3: Second page returns remaining items ───────────────────────────
        [Fact]
        public async Task GetAllCourses_SecondPage_ReturnsRemainingItems()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var result = await service.GetAllCoursesAsync(new CourseFilterRequest { Page = 2, PageSize = 2 });

            Assert.Equal(1, result.Data.Count());
        }

        // ── Test 4: Filter by search term finds matching courses ──────────────────
        [Fact]
        public async Task GetAllCourses_SearchTermFilter_ReturnsMatchingCourses()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var result = await service.GetAllCoursesAsync(new CourseFilterRequest { SearchTerm = "entity" });

            Assert.Equal(1, result.Total);
            Assert.Contains(result.Data, c => c.Title.Contains("Entity Framework"));
        }

        // ── Test 5: Filter by min credits ────────────────────────────────────────
        [Fact]
        public async Task GetAllCourses_MinCreditsFilter_ReturnsOnlyHighCreditCourses()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var result = await service.GetAllCoursesAsync(new CourseFilterRequest { MinCredits = 3 });

            Assert.All(result.Data, c => Assert.True(c.Credits >= 3));
            Assert.Equal(2, result.Total);
        }

        // ── Test 6: GetById returns correct course ────────────────────────────────
        [Fact]
        public async Task GetCourseById_ExistingId_ReturnsCourse()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var course = await service.GetCourseByIdAsync(101);

            Assert.NotNull(course);
            Assert.Equal("ASP.NET Core Basics", course!.Title);
        }

        // ── Test 7: GetById with non-existent ID returns null ────────────────────
        [Fact]
        public async Task GetCourseById_NonExistentId_ReturnsNull()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var course = await service.GetCourseByIdAsync(9999);

            Assert.Null(course);
        }

        // ── Test 8: Soft delete hides course from queries ─────────────────────────
        [Fact]
        public async Task DeleteCourse_SoftDelete_HidesCourseFromQueries()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var deleted = await service.DeleteCourseAsync(101);

            Assert.True(deleted);

            // Course should no longer appear in normal queries (soft deleted)
            var course = await service.GetCourseByIdAsync(101);
            Assert.Null(course);

            var allCourses = await service.GetAllCoursesAsync(new CourseFilterRequest());
            Assert.DoesNotContain(allCourses.Data, c => c.Id == 101);
        }

        // ── Test 9: Create course persists to database ────────────────────────────
        [Fact]
        public async Task CreateCourse_ValidDto_PersistsCourse()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var created = await service.CreateCourseAsync(new CourseCreateDto
            {
                Title = "New Test Course",
                Description = "A fresh course",
                Credits = 3,
                InstructorId = 10
            });

            Assert.NotNull(created);
            Assert.Equal("New Test Course", created.Title);
            Assert.True(created.Id > 0);
        }

        // ── Test 10: Update course changes title ──────────────────────────────────
        [Fact]
        public async Task UpdateCourse_ValidId_UpdatesTitle()
        {
            using var context = TestDbContextFactory.Create();
            await SeedAsync(context);

            var service = new CourseService(context);
            var updated = await service.UpdateCourseAsync(102, new CourseUpdateDto
            {
                Title = "Updated EF Course"
            });

            Assert.True(updated);

            var course = await service.GetCourseByIdAsync(102);
            Assert.Equal("Updated EF Course", course!.Title);
        }
    }
}
