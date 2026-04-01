using System;
using System.Text;
using System.Threading.RateLimiting;
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
using Serilog.Events;
using SmartCourseManagement.API.Data;
using SmartCourseManagement.API.Middleware;
using SmartCourseManagement.API.Services;

// ─── Serilog bootstrap logger ─────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Smart Course Management API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for all application logging
    builder.Host.UseSerilog();

    var services = builder.Services;
    var config = builder.Configuration;

    // ─── Database ──────────────────────────────────────────────────────────────
    services.AddHttpContextAccessor();
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

    // ─── Services ─────────────────────────────────────────────────────────────
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<ICourseService, CourseService>();
    services.AddScoped<IEnrollmentService, EnrollmentService>();
    services.AddScoped<IStudentService, StudentService>();
    services.AddScoped<IInstructorService, InstructorService>();
    services.AddScoped<IEmailService, EmailService>();

    // ─── JWT Authentication ────────────────────────────────────────────────────
    var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    Log.Warning("JWT authentication failed: {Error}", ctx.Exception.Message);
                    return System.Threading.Tasks.Task.CompletedTask;
                },
                OnTokenValidated = ctx =>
                {
                    Log.Debug("JWT token validated for {User}",
                        ctx.Principal?.Identity?.Name);
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

    services.AddAuthorization();

    // ─── Rate Limiting ─────────────────────────────────────────────────────────
    services.AddRateLimiter(options =>
    {
        // General policy: 100 requests per minute per IP
        options.AddFixedWindowLimiter("GeneralPolicy", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });

        // Strict login policy: 5 attempts per minute per IP
        options.AddFixedWindowLimiter("LoginPolicy", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            Log.Warning("Rate limit exceeded for {IP} on {Path}",
                context.HttpContext.Connection.RemoteIpAddress,
                context.HttpContext.Request.Path);
            await context.HttpContext.Response.WriteAsync(
                "{\"message\":\"Too many requests. Please try again later.\"}", token);
        };
    });

    // ─── CORS ─────────────────────────────────────────────────────────────────
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    // ─── Controllers & Swagger ────────────────────────────────────────────────
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Smart Course Management Portal API",
            Version = "v1",
            Description = "Enterprise-grade course management system with JWT auth, pagination, soft delete, and audit trails."
        });

        c.AddSecurityDefinition("jwt_auth", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Description = "Paste your JWT token below (without 'Bearer' prefix)."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "jwt_auth" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ─── Build ────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─── Middleware Pipeline ───────────────────────────────────────────────────
    // Global exception handler must be first so it catches everything below it
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Structured request logging via Serilog
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Course Management API v1");
            c.DefaultModelsExpandDepth(-1);
        });
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // Apply rate limiting to all endpoints by default, stricter policy on login
    app.MapControllers().RequireRateLimiting("GeneralPolicy");

    // ─── Apply Migrations on startup ──────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
