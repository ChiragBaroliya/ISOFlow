using System.Reflection;
using System.Text;
using System.Text.Json;
using ISOFlow.Api.Controllers;
using ISOFlow.Api.Hubs;
using ISOFlow.Api.Middlewares;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Application.Services;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

// Configure Serilog Date-Wise Daily Logger (format: 31082026.log / ddMMyyyy.log)
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

var currentDayLogPath = Path.Combine(logDirectory, $"{DateTime.UtcNow:ddMMyyyy}.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logDirectory, ".log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: currentDayLogPath,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        shared: true
    )
    .CreateLogger();

try
{
    Log.Information("Starting ISOFlow.Api Web Host...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Bind JWT Configuration Settings
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    builder.Services.AddSingleton(jwtSettings);

    // Add MemoryCache & SignalR
    builder.Services.AddMemoryCache();
    builder.Services.AddSignalR();

    // Register Database Connection Factory
    builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

    // Register Repositories
    builder.Services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
    builder.Services.AddSingleton<IUserRepository, UserRepository>();
    builder.Services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddSingleton<IStandardRepository, StandardRepository>();
    builder.Services.AddSingleton<IControlRepository, ControlRepository>();
    builder.Services.AddSingleton<IRiskRepository, RiskRepository>();
    builder.Services.AddSingleton<IDocumentRepository, DocumentRepository>();
    builder.Services.AddSingleton<ITaskRepository, TaskRepository>();
    builder.Services.AddSingleton<IEvidenceRepository, EvidenceRepository>();
    builder.Services.AddSingleton<IAuditRepository, AuditRepository>();
    builder.Services.AddSingleton<ICapaRepository, CapaRepository>();
    builder.Services.AddSingleton<IManagementReviewRepository, ManagementReviewRepository>();
    builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();
    builder.Services.AddSingleton<ITraceabilityRepository, TraceabilityRepository>();

    // Register Application Services
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    builder.Services.AddSingleton<IDashboardService, DashboardService>();
    builder.Services.AddSingleton<ITraceabilityService, TraceabilityService>();
    builder.Services.AddSingleton<ILogService, LogService>();
    builder.Services.AddSingleton<IJwtService, JwtService>();
    builder.Services.AddSingleton<IAuthService, AuthService>();

    // Configure JWT Bearer Authentication
    var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Secret);
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set true in strict production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true
        };

        // Standardize 401 and 403 API responses to match ApiResponse<T> format
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                // Suppress default challenge response to write custom JSON
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var failureResponse = ApiResponse<object>.FailureResponse(
                    "You are not authorized to access this resource. Please provide a valid Bearer token.",
                    new[] { "Unauthorized: Missing, invalid, or expired JWT token." });

                await context.Response.WriteAsync(JsonSerializer.Serialize(failureResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var failureResponse = ApiResponse<object>.FailureResponse(
                    "Access denied. You do not possess the required role/permissions for this resource.",
                    new[] { "Forbidden: Insufficient role permissions." });

                await context.Response.WriteAsync(JsonSerializer.Serialize(failureResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
        };
    });

    builder.Services.AddAuthorization();

    // Configure Controllers with Standardized Model Validation Error Factory
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid input." : x.ErrorMessage))
                    .ToList();

                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed for one or more fields.",
                    Errors = errors,
                    Data = null
                };

                return new BadRequestObjectResult(response);
            };
        });

    builder.Services.AddEndpointsApiExplorer();

    // Configure Swagger OpenAPI Specification with Bearer Token Authorization Support
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ISOFlow Compliance Management Platform API",
            Version = "v1",
            Description = "Enterprise RESTful Web APIs for ISO 27001 / ISO 9001 / ISO 14001 compliance management, Statement of Applicability (SoA), risk assessment, CAPA workflows, audit programs, and date-wise Serilog inspection with JWT Authentication.",
            Contact = new OpenApiContact
            {
                Name = "Acme InfoSec Compliance Engineering",
                Email = "compliance@acme.com"
            }
        });

        // Add Bearer JWT Authentication definition to Swagger
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token format: `Bearer {your_token}`",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
        c.AddSecurityDefinition("Bearer", securityScheme);

        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", null);
        var securityRequirement = new OpenApiSecurityRequirement
        {
            [schemeRef] = new List<string>()
        };
        c.AddSecurityRequirement(doc => securityRequirement);

        // Include XML documentation comments if present
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Eliminate duplicate response status codes in Swagger UI
        c.OperationFilter<ISOFlow.Api.Filters.SwaggerResponseDeduplicationFilter>();
    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
        });
    });

    var app = builder.Build();

    // Global Error Management Middleware
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Enable Swagger in All Environments
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISOFlow Compliance API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "ISOFlow Compliance API Explorer";
    });

    app.UseCors();
    app.UseRouting();

    // Authentication & Authorization Middlewares
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ComplianceHub>("/hubs/compliance");

    // Redirect root to Swagger UI
    app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ISOFlow.Api host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
