using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SmartCourseManagement.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware. Catches all unhandled exceptions and returns
    /// a standardized JSON error response so controllers never need try/catch boilerplate.
    /// Must be registered early in the pipeline (before UseRouting) in Program.cs.
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
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message, errors) = exception switch
            {
                DbUpdateException dbEx => (
                    HttpStatusCode.Conflict,
                    "A database error occurred.",
                    new List<string> { dbEx.InnerException?.Message ?? dbEx.Message }),

                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "Unauthorized access.",
                    new List<string> { exception.Message }),

                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "The requested resource was not found.",
                    new List<string> { exception.Message }),

                ArgumentException => (
                    HttpStatusCode.BadRequest,
                    "Invalid argument.",
                    new List<string> { exception.Message }),

                _ => (
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.",
                    new List<string> { exception.Message })
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                statusCode = (int)statusCode,
                message,
                errors
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
