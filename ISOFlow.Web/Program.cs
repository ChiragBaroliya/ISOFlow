using ISOFlow.Application.Interfaces;
using ISOFlow.Application.Services;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
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
    Log.Information("Starting ISOFlow.Web Application Host...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // MemoryCache
    builder.Services.AddMemoryCache();

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

    // Session Support for Multi-Tenant Org Switcher
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Register MVC Controllers with Views
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ISOFlow.Web host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
