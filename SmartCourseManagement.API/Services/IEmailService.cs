using System.Threading.Tasks;

namespace SmartCourseManagement.API.Services
{
    /// <summary>
    /// Interface for email notification service.
    /// Implementations can send real emails (SMTP/SendGrid) or log to console for testing.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>Sends a welcome email to a newly registered user.</summary>
        Task SendWelcomeEmailAsync(string toEmail, string userName);

        /// <summary>Sends an enrollment confirmation to a student.</summary>
        Task SendEnrollmentConfirmationAsync(string toEmail, string studentName, string courseTitle);

        /// <summary>Sends a generic email.</summary>
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
