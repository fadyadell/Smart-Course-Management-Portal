using System;
using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.Middleware;
using SmartCourseManagement.API.Services;

// =====================================================
// Serilog: configure before the host is built
// =====================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Smart Course Management Portal");

    var builder = WebApplication.CreateBuilder(args);

    // =====================================================
    // Serilog as the logging provider
    // =====================================================
    builder.Host.UseSerilog();

    // =====================================================
    // Database
    // =====================================================
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // =====================================================
    // JWT Authentication
    // =====================================================
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key is not configured.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // No clock skew — tokens expire exactly at ExpiryMinutes
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT token validated for {User}", context.Principal?.Identity?.Name);
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // =====================================================
    // Application Services (DI)
    // =====================================================
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICourseService, CourseService>();
    builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
    builder.Services.AddScoped<IInstructorService, InstructorService>();
    builder.Services.AddScoped<IStudentService, StudentService>();
    builder.Services.AddScoped<BackgroundJobService>();

    // =====================================================
    // Rate Limiting
    // =====================================================
    builder.Services.AddRateLimiter(options =>
    {
        // Policy 1: 100 requests per minute per IP (general)
        options.AddPolicy("GeneralPolicy", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // Policy 2: 5 login attempts per minute per IP
        options.AddPolicy("LoginPolicy", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        options.OnRejected = async (context, _) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
        };
    });

    // =====================================================
    // Hangfire Background Jobs
    // =====================================================
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));
    builder.Services.AddHangfireServer();

    // =====================================================
    // Controllers + XML docs
    // =====================================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // =====================================================
    // Swagger with JWT Bearer support
    // =====================================================
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Smart Course Management Portal API",
            Version = "v1",
            Description = "A full-featured course management API with JWT authentication, " +
                          "refresh tokens, Hangfire background jobs, Serilog logging, " +
                          "rate limiting, and pagination."
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Description = "Enter your JWT token. Example: Bearer {your_token}"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // =====================================================
    // CORS
    // =====================================================
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // =====================================================
    // Middleware pipeline
    // =====================================================

    // Global exception handler (first in pipeline)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Serilog request logging
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Course Management API V1");
            c.DefaultModelsExpandDepth(-1);
            c.DisplayRequestDuration();
        });
    }

    app.UseStaticFiles();
    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // =====================================================
    // Hangfire Dashboard (accessible only in development)
    // =====================================================
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire");
    }

    app.MapControllers();

    // =====================================================
    // Register Hangfire recurring jobs
    // =====================================================
    try
    {
        BackgroundJobService.RegisterJobs();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Could not register Hangfire jobs (database may not be available)");
    }

    Log.Information("Application started. Listening on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}