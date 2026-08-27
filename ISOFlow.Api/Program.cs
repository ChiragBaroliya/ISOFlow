using System.Reflection;
using ISOFlow.Api.Hubs;
using ISOFlow.Api.Middlewares;
using ISOFlow.Application.Interfaces;
using ISOFlow.Application.Services;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.OpenApi;
using Serilog;

// Configure Serilog Date-Wise Rolling Daily Logger
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logDirectory, "isoflow-.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("Starting ISOFlow.Api Web Host...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add MemoryCache & SignalR
    builder.Services.AddMemoryCache();
    builder.Services.AddSignalR();

    // Register Repositories
    builder.Services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
    builder.Services.AddSingleton<IUserRepository, UserRepository>();
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

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure Swagger OpenAPI Specification
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ISOFlow Compliance Management Platform API",
            Version = "v1",
            Description = "RESTful Web APIs for ISO 27001 / ISO 9001 compliance traceability, risks, controls, and date-wise log management.",
            Contact = new OpenApiContact
            {
                Name = "Acme InfoSec Team",
                Email = "compliance@acme.com"
            }
        });

        // Include XML documentation comments if present
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
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

    // Enable Swagger in All Environments for Testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISOFlow Compliance API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ComplianceHub>("/hubs/compliance");

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
