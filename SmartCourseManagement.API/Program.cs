using System;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.Middleware;
using SmartCourseManagement.API.Services;

// ─────────────────────────────────────────────────────────────────────────────
// Configure Serilog early so startup errors are captured in the log file too.
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for all ASP.NET Core logging
    builder.Host.UseSerilog();

    // ─────────────────────────────────────────────────────────────────────────
    // Database
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

    // ─────────────────────────────────────────────────────────────────────────
    // Dependency Injection – Services
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICourseService, CourseService>();
    builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
    builder.Services.AddScoped<IStudentService, StudentService>();
    builder.Services.AddScoped<IInstructorService, InstructorService>();

    // ─────────────────────────────────────────────────────────────────────────
    // JWT Authentication
    // ─────────────────────────────────────────────────────────────────────────
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services.AddAuthorization();

    // ─────────────────────────────────────────────────────────────────────────
    // API Versioning
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // ─────────────────────────────────────────────────────────────────────────
    // Rate Limiting – 100 requests per minute per IP
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("fixed", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });
        options.RejectionStatusCode = 429;
    });

    // ─────────────────────────────────────────────────────────────────────────
    // CORS – allow all origins (lock down in production)
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    // ─────────────────────────────────────────────────────────────────────────
    // Swagger / OpenAPI with JWT bearer support
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Smart Course Management API",
            Version = "v1",
            Description = "REST API for managing courses, enrollments, and users. " +
                          "Authenticate via POST /api/v1/auth/login, copy the returned token, " +
                          "and click the Authorize button above."
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Description = "Enter your JWT token."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddControllers();

    // ─────────────────────────────────────────────────────────────────────────
    // Build & configure the HTTP pipeline
    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // Global exception handler – must be first in the pipeline
    app.UseMiddleware<ExceptionMiddleware>();

    // Serilog request logging
    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Course Management API v1");
            c.RoutePrefix = "swagger";
            c.DefaultModelsExpandDepth(-1);
        });
    }

    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers().RequireRateLimiting("fixed");

    Log.Information("Smart Course Management API starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
