using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartCourseManagement.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware that catches all unhandled exceptions
    /// and returns a standardized JSON error response.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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
                _logger.LogError($"Unhandled exception: {ex.Message}\n{ex.StackTrace}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new ErrorResponse
            {
                Message = "An unexpected error occurred. Please try again later.",
                StatusCode = StatusCodes.Status500InternalServerError,
                Timestamp = DateTime.UtcNow
            };

            // Add detailed error info in development
            var hostEnvironment = context.RequestServices.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
            if (hostEnvironment?.IsDevelopment() == true)
            {
                response.Details = exception.Message;
                response.StackTrace = exception.StackTrace;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>
    /// Standard error response format for all API errors.
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
        public string StackTrace { get; set; }
    }
}
