using System;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SmartCourseManagement.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware. Catches all unhandled exceptions and returns
    /// a standardized JSON error response with the appropriate HTTP status code.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        // Allowlist for safe log characters: letters, digits, /, ., -, _, ~
        private static readonly Regex _safePathChars = new Regex(@"[^a-zA-Z0-9/.\-_~]", RegexOptions.Compiled);

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                // Use allowlist sanitization to produce a safe string before logging
                string rawMethod = context.Request.Method ?? string.Empty;
                string rawPath   = context.Request.Path.ToString() ?? string.Empty;
                string safeMethod = _safePathChars.Replace(rawMethod, "_");
                string safePath   = _safePathChars.Replace(rawPath, "_");

                _logger.LogError(ex, "Unhandled exception for request {Method} {Path}", safeMethod, safePath);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
                DbUpdateException dbEx => (HttpStatusCode.Conflict, "A database error occurred: " + GetDbErrorMessage(dbEx)),
                ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
                InvalidOperationException invEx => (HttpStatusCode.BadRequest, invEx.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                message,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static string GetDbErrorMessage(DbUpdateException ex)
        {
            // Check for unique constraint violation
            if (ex.InnerException?.Message?.Contains("UNIQUE") == true ||
                ex.InnerException?.Message?.Contains("unique") == true ||
                ex.InnerException?.Message?.Contains("duplicate") == true)
            {
                return "A record with this value already exists.";
            }
            return "The operation could not be completed due to a database constraint.";
        }
    }
}
