using ISOFlow.Web.Services.Audits;
using ISOFlow.Web.Services.Auth;
using ISOFlow.Web.Services.Base;
using ISOFlow.Web.Services.Capa;
using ISOFlow.Web.Services.Controls;
using ISOFlow.Web.Services.Dashboard;
using ISOFlow.Web.Services.Documents;
using ISOFlow.Web.Services.Evidence;
using ISOFlow.Web.Services.Findings;
using ISOFlow.Web.Services.Improvements;
using ISOFlow.Web.Services.Logs;
using ISOFlow.Web.Services.ManagementReviews;
using ISOFlow.Web.Services.Organizations;
using ISOFlow.Web.Services.Risks;
using ISOFlow.Web.Services.Standards;
using ISOFlow.Web.Services.Tasks;
using ISOFlow.Web.Services.Users;
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

    // HttpContextAccessor (needed by ApiHttpClient to read Session JWT token)
    builder.Services.AddHttpContextAccessor();

    // Register Base Typed HttpClient pointing to ISOFlow.Api
    builder.Services.AddHttpClient<IApiHttpClient, ApiHttpClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5015");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    // Register Domain-Specific API Clients (separated by functional area)
    builder.Services.AddScoped<IDashboardApiClient, DashboardApiClient>();
    builder.Services.AddScoped<IStandardsApiClient, StandardsApiClient>();
    builder.Services.AddScoped<IControlsApiClient, ControlsApiClient>();
    builder.Services.AddScoped<IRisksApiClient, RisksApiClient>();
    builder.Services.AddScoped<IDocumentsApiClient, DocumentsApiClient>();
    builder.Services.AddScoped<ITasksApiClient, TasksApiClient>();
    builder.Services.AddScoped<IEvidenceApiClient, EvidenceApiClient>();
    builder.Services.AddScoped<IAuditsApiClient, AuditsApiClient>();
    builder.Services.AddScoped<IFindingsApiClient, FindingsApiClient>();
    builder.Services.AddScoped<ICapaApiClient, CapaApiClient>();
    builder.Services.AddScoped<IManagementReviewsApiClient, ManagementReviewsApiClient>();
    builder.Services.AddScoped<IImprovementsApiClient, ImprovementsApiClient>();
    builder.Services.AddScoped<IUsersApiClient, UsersApiClient>();
    builder.Services.AddScoped<IOrganizationsApiClient, OrganizationsApiClient>();
    builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
    builder.Services.AddScoped<ILogsApiClient, LogsApiClient>();

    // Session Support for Multi-Tenant Org Switcher and JWT Token Storage
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Register MVC Controllers with Views and Global Session Authorization Filter
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<ISOFlow.Web.Filters.SessionAuthorizeAttribute>();
    });

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
