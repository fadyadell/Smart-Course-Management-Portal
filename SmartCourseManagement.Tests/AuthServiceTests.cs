using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Models;
using SmartCourseManagement.API.Services;
using Xunit;

namespace SmartCourseManagement.Tests
{
    /// <summary>
    /// Helper that creates an in-memory AppDbContext for unit tests.
    /// A unique database name per test ensures test isolation.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static AppDbContext Create(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            return new AppDbContext(options, httpContextAccessor.Object);
        }
    }

    /// <summary>
    /// Unit tests for AuthService covering registration, login, duplicate detection,
    /// password hashing, and token refresh.
    /// </summary>
    public class AuthServiceTests
    {
        private IConfiguration BuildConfiguration()
        {
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretTestKeyThatIsLongEnough123!456",
                ["Jwt:Issuer"] = "SmartCourseManagement",
                ["Jwt:Audience"] = "SmartCourseManagementUsers"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private AuthService CreateService(AppDbContext context)
        {
            var config = BuildConfiguration();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<AuthService>>();

            return new AuthService(context, config, emailMock.Object, logger.Object);
        }

        // ── Test 1: Successful student registration returns token ─────────────────
        [Fact]
        public async Task RegisterAsync_ValidStudent_ReturnsTokenAndUserInfo()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            var dto = new UserRegisterDto
            {
                Name = "Alice Smith",
                Email = "alice@test.com",
                Password = "Password123",
                Role = "Student"
            };

            var result = await service.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);
            Assert.Equal("alice@test.com", result.User.Email);
            Assert.Equal("Student", result.User.Role);
        }

        // ── Test 2: Registering instructor auto-creates InstructorProfile ─────────
        [Fact]
        public async Task RegisterAsync_InstructorRole_CreatesInstructorProfile()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Dr. Bob",
                Email = "bob@test.com",
                Password = "Password123",
                Role = "Instructor"
            });

            var profile = await context.InstructorProfiles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.User.Email == "bob@test.com");

            Assert.NotNull(profile);
        }

        // ── Test 3: Duplicate email throws exception ──────────────────────────────
        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            var dto = new UserRegisterDto
            {
                Name = "Alice",
                Email = "alice@test.com",
                Password = "Password123",
                Role = "Student"
            };

            await service.RegisterAsync(dto);

            // Second registration with same email must throw
            var ex = await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(dto));
            Assert.Contains("already exists", ex.Message);
        }

        // ── Test 4: Login with correct credentials returns token ──────────────────
        [Fact]
        public async Task LoginAsync_CorrectCredentials_ReturnsToken()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Carol",
                Email = "carol@test.com",
                Password = "MyPass123",
                Role = "Student"
            });

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "carol@test.com",
                Password = "MyPass123"
            });

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);
        }

        // ── Test 5: Login with wrong password returns null ────────────────────────
        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Dave",
                Email = "dave@test.com",
                Password = "RealPassword",
                Role = "Student"
            });

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "dave@test.com",
                Password = "WrongPassword"
            });

            Assert.Null(result);
        }

        // ── Test 6: Login with non-existent email returns null ────────────────────
        [Fact]
        public async Task LoginAsync_NonExistentEmail_ReturnsNull()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "ghost@test.com",
                Password = "AnyPassword"
            });

            Assert.Null(result);
        }

        // ── Test 7: Password hashing and verification ─────────────────────────────
        [Fact]
        public void HashPassword_ThenVerify_ReturnsTrue()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            var plainText = "TestPassword123!";
            var hash = service.HashPassword(plainText);

            Assert.True(service.VerifyPassword(plainText, hash));
            Assert.False(service.VerifyPassword("WrongPassword", hash));
        }

        // ── Test 8: Refresh token exchange returns new tokens ─────────────────────
        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokenPair()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            var authResult = await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Eve",
                Email = "eve@test.com",
                Password = "Password123",
                Role = "Student"
            });

            var refreshResult = await service.RefreshTokenAsync(authResult.RefreshToken);

            Assert.NotNull(refreshResult);
            Assert.NotEmpty(refreshResult!.Token);
            Assert.NotEmpty(refreshResult.RefreshToken);
            // New refresh token must be different from the old one
            Assert.NotEqual(authResult.RefreshToken, refreshResult.RefreshToken);
        }

        // ── Test 9: Replaying a used refresh token returns null ───────────────────
        [Fact]
        public async Task RefreshTokenAsync_UsedToken_ReturnsNull()
        {
            using var context = TestDbContextFactory.Create();
            var service = CreateService(context);

            var authResult = await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Frank",
                Email = "frank@test.com",
                Password = "Password123",
                Role = "Student"
            });

            // First use — should succeed
            await service.RefreshTokenAsync(authResult.RefreshToken);

            // Second use of same token — must be rejected
            var result = await service.RefreshTokenAsync(authResult.RefreshToken);
            Assert.Null(result);
        }
    }
}
