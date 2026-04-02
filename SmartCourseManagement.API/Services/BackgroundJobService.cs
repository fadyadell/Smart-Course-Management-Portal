using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.Models;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Background job service for scheduled tasks via Hangfire
    /// </summary>
    public class BackgroundJobService
    {
        private readonly AppDbContext _context;

        public BackgroundJobService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Auto-unenroll students from expired courses (marked as deleted).
        /// This job runs daily to clean up enrollment records for inactive courses.
        /// </summary>
        public async Task AutoUnenrollFromExpiredCoursesAsync()
        {
            // Find all enrollments where the course is soft-deleted or more than 1 year old
            var expiredEnrollments = await _context.Enrollments
                .Where(e => e.Course.IsDeleted || e.CreatedAt < DateTime.UtcNow.AddYears(-1))
                .ToListAsync();

            if (expiredEnrollments.Any())
            {
                foreach (var enrollment in expiredEnrollments)
                {
                    enrollment.IsDeleted = true;
                    enrollment.DeletedAt = DateTime.UtcNow;
                    enrollment.DeletedBy = "System";
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Unenrolled {expiredEnrollments.Count} students from expired courses");
            }
        }

        /// <summary>
        /// Generate weekly enrollment summary report.
        /// This job runs every Monday to create enrollment statistics.
        /// </summary>
        public async Task GenerateWeeklyEnrollmentReportAsync()
        {
            var lastWeekStart = DateTime.UtcNow.AddDays(-7);
            
            var weeklyEnrollments = await _context.Enrollments
                .Where(e => e.CreatedAt >= lastWeekStart && !e.IsDeleted)
                .GroupBy(e => e.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    CourseCount = g.Count(),
                    EnrollmentDate = g.Max(e => e.CreatedAt)
                })
                .ToListAsync();

            var report = new
            {
                ReportDate = DateTime.UtcNow,
                TotalNewEnrollments = weeklyEnrollments.Count,
                TotalCourseEnrollments = weeklyEnrollments.Sum(w => w.CourseCount),
                TopStudents = weeklyEnrollments.OrderByDescending(w => w.CourseCount).Take(5)
            };

            Console.WriteLine($"Weekly Report Generated: {report.TotalNewEnrollments} new enrollments, {report.TotalCourseEnrollments} total courses");
        }

        /// <summary>
        /// Revoke old refresh tokens (older than 30 days).
        /// </summary>
        public async Task RevokeOldRefreshTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.CreatedAt < DateTime.UtcNow.AddDays(-30) && !rt.IsRevoked)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                foreach (var token in expiredTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Revoked {expiredTokens.Count} old refresh tokens");
            }
        }
    }
}
