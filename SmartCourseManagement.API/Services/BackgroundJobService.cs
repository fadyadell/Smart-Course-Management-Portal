using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCourseManagement.API.Data;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Background jobs registered with Hangfire.
    /// Jobs are resolved via DI from the service provider.
    /// </summary>
    public class BackgroundJobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundJobService> _logger;

        public BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Registers all recurring background jobs with Hangfire.
        /// Call this once during application startup.
        /// </summary>
        public static void RegisterJobs()
        {
            // Run daily at midnight: clean up expired refresh tokens
            RecurringJob.AddOrUpdate<BackgroundJobService>(
                "cleanup-expired-tokens",
                job => job.CleanupExpiredRefreshTokensAsync(),
                Cron.Daily);

            // Run weekly on Monday at 8am: enrollment summary report
            RecurringJob.AddOrUpdate<BackgroundJobService>(
                "weekly-enrollment-summary",
                job => job.GenerateWeeklyEnrollmentSummaryAsync(),
                Cron.Weekly(DayOfWeek.Monday, 8));
        }

        /// <summary>
        /// Removes expired and revoked refresh tokens from the database.
        /// Scheduled to run daily.
        /// </summary>
        public async Task CleanupExpiredRefreshTokensAsync()
        {
            _logger.LogInformation("Running CleanupExpiredRefreshTokens job at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow;
            var expiredTokens = await context.RefreshTokens
                .Where(rt => rt.ExpiryDate < cutoff || rt.IsRevoked)
                .ToListAsync();

            context.RefreshTokens.RemoveRange(expiredTokens);
            await context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", expiredTokens.Count);
        }

        /// <summary>
        /// Generates a weekly summary report of new enrollments.
        /// Scheduled to run every Monday at 8am.
        /// </summary>
        public async Task GenerateWeeklyEnrollmentSummaryAsync()
        {
            _logger.LogInformation("Running WeeklyEnrollmentSummary job at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var weekStart = DateTime.UtcNow.AddDays(-7);
            var enrollmentCount = await context.Enrollments
                .Where(e => e.EnrollmentDate >= weekStart)
                .CountAsync();

            var courseBreakdown = await context.Enrollments
                .Where(e => e.EnrollmentDate >= weekStart)
                .GroupBy(e => e.Course.Title)
                .Select(g => new { Course = g.Key, Count = g.Count() })
                .ToListAsync();

            _logger.LogInformation(
                "Weekly Enrollment Summary: {Total} new enrollments in the past 7 days",
                enrollmentCount);

            foreach (var item in courseBreakdown)
            {
                _logger.LogInformation("  - {Course}: {Count} new enrollments", item.Course, item.Count);
            }
        }
    }
}
