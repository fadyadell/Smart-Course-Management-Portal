using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartCourseManagement.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware. Catches any unhandled exception from the
    /// pipeline and returns a consistent JSON error response instead of an HTML error page.
    /// This keeps error responses clean and machine-readable.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Sanitize user-controlled values before logging to prevent log forging
                var safeMethod = context.Request.Method.Replace('\r', ' ').Replace('\n', ' ');
                var safePath = context.Request.Path.Value?.Replace('\r', ' ').Replace('\n', ' ') ?? string.Empty;
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", safeMethod, safePath);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized to perform this action."),
                ArgumentException e => (HttpStatusCode.BadRequest, e.Message),
                InvalidOperationException e => (HttpStatusCode.BadRequest, e.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                statusCode = (int)statusCode,
                message,
                traceId = context.TraceIdentifier
            });

            return context.Response.WriteAsync(result);
        }
    }
}
