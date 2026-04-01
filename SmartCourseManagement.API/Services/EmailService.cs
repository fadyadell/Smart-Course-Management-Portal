using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Mock email service implementation that logs emails to the console and application log.
    /// In production replace with an SMTP or SendGrid implementation by swapping the DI registration.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Smart Course Management Portal!";
            var body = $@"
Dear {userName},

Welcome to the Smart Course Management Portal!

Your account has been successfully created. You can now log in and explore our courses.

Best regards,
Smart Course Management Team";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEnrollmentConfirmationAsync(string toEmail, string studentName, string courseTitle)
        {
            var subject = $"Enrollment Confirmation: {courseTitle}";
            var body = $@"
Dear {studentName},

You have successfully enrolled in: {courseTitle}

You can view your enrollments in the portal at any time.

Best regards,
Smart Course Management Team";

            await SendEmailAsync(toEmail, subject, body);
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // In a real app this would use MailKit or SendGrid.
            // For now we log the email details so tests and dev runs can verify behaviour.
            _logger.LogInformation(
                "📧 EMAIL SENT | To: {To} | Subject: {Subject} | Body: {Body}",
                toEmail, subject, body);

            Console.WriteLine($"[EMAIL] To: {toEmail} | Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
