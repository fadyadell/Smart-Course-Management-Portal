using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.DTOs;
using SmartCourseManagement.API.Services;
using Xunit;

namespace SmartCourseManagement.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthService using an in-memory SQLite-style EF Core database.
    /// Tests cover registration, login, duplicate email handling, and token refresh.
    /// </summary>
    public class AuthServiceTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private IConfiguration BuildConfig()
        {
            var inMemory = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Jwt:Key"]      = "TestSecretKeyThatIsLongEnoughForHmac256!",
                ["Jwt:Issuer"]   = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };
            return new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
        }

        [Fact]
        public async Task Register_NewUser_ReturnsTokenAndUser()
        {
            using var ctx = CreateContext(nameof(Register_NewUser_ReturnsTokenAndUser));
            var service = new AuthService(ctx, BuildConfig());

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

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsException()
        {
            using var ctx = CreateContext(nameof(Register_DuplicateEmail_ThrowsException));
            var service = new AuthService(ctx, BuildConfig());

            var dto = new UserRegisterDto
            {
                Name = "Alice Smith",
                Email = "alice@test.com",
                Password = "Password123",
                Role = "Student"
            };

            await service.RegisterAsync(dto);

            await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(dto));
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenAndUser()
        {
            using var ctx = CreateContext(nameof(Login_ValidCredentials_ReturnsTokenAndUser));
            var service = new AuthService(ctx, BuildConfig());

            var register = new UserRegisterDto
            {
                Name = "Bob Jones",
                Email = "bob@test.com",
                Password = "Secret123",
                Role = "Student"
            };
            await service.RegisterAsync(register);

            var login = new LoginDto { Email = "bob@test.com", Password = "Secret123" };
            var result = await service.LoginAsync(login);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("bob@test.com", result.User.Email);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(Login_WrongPassword_ReturnsNull));
            var service = new AuthService(ctx, BuildConfig());

            await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Carol", Email = "carol@test.com", Password = "RightPass1", Role = "Student"
            });

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "carol@test.com",
                Password = "WrongPass"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task Login_UnknownEmail_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(Login_UnknownEmail_ReturnsNull));
            var service = new AuthService(ctx, BuildConfig());

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "nobody@test.com",
                Password = "Any"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshToken_ValidToken_ReturnsNewTokenPair()
        {
            using var ctx = CreateContext(nameof(RefreshToken_ValidToken_ReturnsNewTokenPair));
            var service = new AuthService(ctx, BuildConfig());

            var reg = await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Dave", Email = "dave@test.com", Password = "Pass123!", Role = "Student"
            });

            var refreshed = await service.RefreshTokenAsync(reg.RefreshToken);

            Assert.NotNull(refreshed);
            Assert.NotEmpty(refreshed!.Token);
            Assert.NotEmpty(refreshed.RefreshToken);
            Assert.NotEqual(reg.RefreshToken, refreshed.RefreshToken); // rotation
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(RefreshToken_InvalidToken_ReturnsNull));
            var service = new AuthService(ctx, BuildConfig());

            var result = await service.RefreshTokenAsync("not-a-valid-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshToken_UsedTokenIsRevoked()
        {
            using var ctx = CreateContext(nameof(RefreshToken_UsedTokenIsRevoked));
            var service = new AuthService(ctx, BuildConfig());

            var reg = await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Eve", Email = "eve@test.com", Password = "Pass123!", Role = "Student"
            });

            // First use — should succeed
            var first = await service.RefreshTokenAsync(reg.RefreshToken);
            Assert.NotNull(first);

            // Second use of same (now revoked) token — should fail
            var second = await service.RefreshTokenAsync(reg.RefreshToken);
            Assert.Null(second);
        }

        [Fact]
        public void HashPassword_ThenVerify_Succeeds()
        {
            using var ctx = CreateContext(nameof(HashPassword_ThenVerify_Succeeds));
            var service = new AuthService(ctx, BuildConfig());

            var hash = service.HashPassword("mypassword");
            Assert.True(service.VerifyPassword("mypassword", hash));
            Assert.False(service.VerifyPassword("wrongpassword", hash));
        }

        [Fact]
        public async Task Register_Instructor_CreatesInstructorProfile()
        {
            using var ctx = CreateContext(nameof(Register_Instructor_CreatesInstructorProfile));
            var service = new AuthService(ctx, BuildConfig());

            await service.RegisterAsync(new UserRegisterDto
            {
                Name = "Prof Frank",
                Email = "frank@test.com",
                Password = "Pass123!",
                Role = "Instructor"
            });

            var profile = await ctx.InstructorProfiles.FirstOrDefaultAsync();
            Assert.NotNull(profile);
        }
    }
}
