using System.Reflection;
using System.Text;
using System.Text.Json;
using ISOFlow.Api;
using ISOFlow.Api.Auditing;
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
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

    // "--migrate": apply database migrations and exit without starting the web host (see below).
    var migrateOnly = args.Contains("--migrate");

    // Bind JWT Configuration Settings. Secrets are never committed: they come from User Secrets in
    // development and from environment variables elsewhere. Fail fast rather than run with a weak or missing key.
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    if (!migrateOnly && (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < JwtSettings.MinimumSecretLength))
    {
        throw new InvalidOperationException(
            $"JwtSettings:Secret must be configured and at least {JwtSettings.MinimumSecretLength} characters. " +
            "Set it with 'dotnet user-secrets set \"JwtSettings:Secret\" \"<random value>\"' (development) " +
            "or the JwtSettings__Secret environment variable.");
    }
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection must be configured via User Secrets (development) " +
            "or the ConnectionStrings__DefaultConnection environment variable.");
    }

    // Database migrations (DbUp, see DatabaseMigrator). "--migrate" applies them and exits (use it as a deploy
    // step); otherwise they run at startup only when Database:ApplyMigrationsOnStartup is set (on in Development).
    // Demo seed data is never applied outside Development, whatever the configuration says.
    if (migrateOnly || builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        var seedRequested = builder.Configuration.GetValue<bool>("Database:SeedDemoData");
        if (seedRequested && !builder.Environment.IsDevelopment())
            Log.Warning("Database:SeedDemoData is ignored outside the Development environment.");

        var migration = DatabaseMigrator.Run(builder.Configuration.GetConnectionString("DefaultConnection")!, new DatabaseMigrationOptions
        {
            CreateDatabaseIfMissing = builder.Configuration.GetValue<bool>("Database:CreateIfMissing"),
            SeedDemoData = seedRequested && builder.Environment.IsDevelopment()
        });
        if (!migration.Successful)
            throw new InvalidOperationException("Database migration failed.", migration.Error);

        foreach (var script in migration.AppliedScripts)
            Log.Information("Applied database script {Script}", script);
        Log.Information("Database is up to date ({Count} script(s) applied this run).", migration.AppliedScripts.Count);

        if (migrateOnly) return;
    }

    builder.Services.AddSingleton(jwtSettings);

    // Add MemoryCache & SignalR
    builder.Services.AddMemoryCache();
    builder.Services.AddSignalR();

    // Register Database Connection Factory
    builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

    // Register Repositories.
    // Singleton is safe here: repositories and services hold no per-request state (only the connection
    // factory and other singletons), each call opens and disposes its own connection, and the shared audit
    // transaction lives in AmbientDbContext's AsyncLocal, which flows per request rather than per instance.
    // Anything that needs per-request state (e.g. HttpContext) must not be added as a singleton.
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
    builder.Services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddSingleton<IComplianceSnapshotRepository, ComplianceSnapshotRepository>();

    // Register Application Services
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    builder.Services.AddSingleton<IDashboardService, DashboardService>();
    builder.Services.AddSingleton<ITraceabilityService, TraceabilityService>();
    builder.Services.AddSingleton<ILogService, LogService>();
    builder.Services.AddSingleton<IJwtService, JwtService>();
    builder.Services.AddSingleton<IAuthService, AuthService>();
    builder.Services.AddSingleton<IAuditLogService, AuditLogService>();

    // Centralized Audit Logging: one line per entity type registers "how to fetch my current state
    // by id" (see AuditSnapshotRegistrations); the global AuditActionFilter uses this so controllers
    // never write audit code themselves — they just carry an [Audit(...)] attribute.
    builder.Services.AddSingleton<IAuditSnapshotRegistry>(_ =>
    {
        var registry = new AuditSnapshotRegistry();
        AuditSnapshotRegistrations.RegisterAll(registry);
        return registry;
    });

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
            ClockSkew = TimeSpan.FromMinutes(1),
            RequireExpirationTime = true
        };

        // Standardize 401 and 403 API responses to match ApiResponse<T> format
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    var token = authHeader.Trim();
                    // Gracefully strip single or duplicated "Bearer " prefixes (e.g. if user pastes 'Bearer eyJ...' in Swagger)
                    while (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = token.Substring(7).Trim();
                    }
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Log.Warning(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                // Suppress default challenge response to write custom JSON
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var failureDetail = context.AuthenticateFailure != null
                    ? $"Unauthorized: {context.AuthenticateFailure.Message}"
                    : "Unauthorized: Missing, invalid, or expired JWT token.";

                var failureResponse = ApiResponse<object>.FailureResponse(
                    "You are not authorized to access this resource. Please provide a valid Bearer token.",
                    new[] { failureDetail });

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

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    // Configure Controllers with Standardized Model Validation Error Factory
    builder.Services.AddControllers(options =>
        {
            // Global, automatic audit logging: acts only on actions carrying [Audit(...)] — see
            // ISOFlow.Api/Auditing/AuditActionFilter.cs. No per-controller audit code required.
            options.Filters.Add<AuditActionFilter>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err => $"{e.Key}: {err.ErrorMessage}"))
                    .ToArray();

                var response = ApiResponse<object>.FailureResponse("Validation failed for one or more fields.", errors);
                return new BadRequestObjectResult(response);
            };
        });

    builder.Services.AddEndpointsApiExplorer();

    // Configure Swagger OpenAPI Specification with Bearer Token Authorization Support
    // Optional API contact shown in Swagger, from "Swagger:Contact:Name" / "Swagger:Contact:Email".
    var swaggerContactName = builder.Configuration["Swagger:Contact:Name"];
    var swaggerContactEmail = builder.Configuration["Swagger:Contact:Email"];

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ISOFlow Compliance Management Platform API",
            Version = "v1",
            Description = "Enterprise RESTful Web APIs for ISO 27001 / ISO 9001 / ISO 14001 compliance management, Statement of Applicability (SoA), risk assessment, CAPA workflows, audit programs, and date-wise Serilog inspection with JWT Authentication.",
            Contact = string.IsNullOrWhiteSpace(swaggerContactName) && string.IsNullOrWhiteSpace(swaggerContactEmail)
                ? null
                : new OpenApiContact
                {
                    Name = swaggerContactName,
                    Email = swaggerContactEmail
                }
        });

        // Add Bearer JWT Authentication definition to Swagger
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token. You can enter just the token (e.g. `eyJ...`) or `Bearer {token}`.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
        c.AddSecurityDefinition("Bearer", securityScheme);

        c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", doc),
                new List<string>()
            }
        });

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

    // CORS: only explicitly configured browser origins (Cors:AllowedOrigins). ISOFlow.Web calls the API
    // server-to-server, so production can leave the list empty; add the Web origin only if pages call the
    // API from the browser (e.g. SignalR).
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            }
        });
    });

    // Rate limiting for anonymous credential endpoints (login, token refresh, password reset), per client IP.
    builder.Services.AddRateLimiter(options =>
    {
        var permitLimit = builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
        var window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60));

        options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit, Window = window, QueueLimit = 0 }));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

            await context.HttpContext.Response.WriteAsJsonAsync(
                ApiResponse<object>.FailureResponse("Too many requests. Please wait and try again."),
                cancellationToken);
        };
    });

    var app = builder.Build();

    // Global Error Management Middleware
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Swagger is opt-in (Swagger:Enabled, on in appsettings.Development.json) so production doesn't publish the API surface.
    var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");
    if (swaggerEnabled)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISOFlow Compliance API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "ISOFlow Compliance API Explorer";
        });
    }

    app.UseCors();
    app.UseRouting();
    app.UseRateLimiter();

    // Authentication & Authorization Middlewares
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ComplianceHub>("/hubs/compliance");

    // Redirect root to Swagger UI when it is enabled
    if (swaggerEnabled)
    {
        app.MapGet("/", () => Results.Redirect("/swagger/index.html")).AllowAnonymous();
    }

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
