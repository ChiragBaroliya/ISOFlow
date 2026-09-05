using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Services;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using Xunit.Abstractions;

namespace ISOFlow.Tests;

public class DatabaseCrudTests
{
    private readonly ITestOutputHelper _output;
    private readonly IDbConnectionFactory _dbFactory;

    public DatabaseCrudTests(ITestOutputHelper output)
    {
        _output = output;
        _dbFactory = new DbConnectionFactory("Host=localhost;Database=isoflow;Username=postgres;Password=postgres;");
    }

    [Fact]
    public async Task StandardRepository_DatabaseCrud_Operations_Succeed()
    {
        var repo = new StandardRepository(_dbFactory);

        // 1. Read
        var all = await repo.GetAllStandardsAsync(1);
        Assert.NotEmpty(all);
        _output.WriteLine($"Total standards in DB: {all.Count}");

        // 2. Create
        var newStd = new Standard
        {
            OrganizationId = 1,
            Code = $"ISO-TEST-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Name = "Temporary Test Standard for DB CRUD",
            Revision = "2026",
            Description = "Automated DB verification",
            CompliancePercentage = 80.0,
            Status = "Active"
        };
        var created = await repo.CreateStandardAsync(newStd);
        Assert.NotNull(created.Id);
        _output.WriteLine($"Created standard in DB with ID: {created.Id}, Code: {created.Code}");

        // 3. Read by ID
        var fetched = await repo.GetStandardByIdAsync(created.Id, 1);
        Assert.NotNull(fetched);
        Assert.Equal(newStd.Code, fetched.Code);

        // 4. Update
        fetched.Name = "Updated Test Standard Title in DB";
        var updated = await repo.UpdateStandardAsync(fetched, 1);
        Assert.NotNull(updated);
        Assert.Equal("Updated Test Standard Title in DB", updated.Name);

        // 5. Delete
        var deleted = await repo.DeleteStandardAsync(created.Id, 1);
        Assert.True(deleted);
        _output.WriteLine("Deleted test standard from DB.");

        var verifyDeleted = await repo.GetStandardByIdAsync(created.Id, 1);
        Assert.Null(verifyDeleted);
    }

    [Fact]
    public async Task ControlRepository_DatabaseCrud_Operations_Succeed()
    {
        var repo = new ControlRepository(_dbFactory);

        // 1. Read
        var all = await repo.GetAllControlsAsync(1);
        Assert.NotEmpty(all);
        _output.WriteLine($"Total controls in DB: {all.Count}");

        // 2. Create
        var newCtrl = new Control
        {
            OrganizationId = 1,
            Code = $"CTRL-T-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Title = "Database CRUD Test Control",
            Category = "Testing Controls",
            Description = "Automated DB verification",
            Status = ControlStatus.InDevelopment,
            Owner = "Test Automation",
            CompliancePercentage = 75.0,
            IsApplicable = true,
            Justification = "Verification"
        };
        var created = await repo.CreateControlAsync(newCtrl);
        Assert.NotNull(created.Id);
        _output.WriteLine($"Created control in DB with ID: {created.Id}, Code: {created.Code}");

        // 3. Read by ID
        var fetched = await repo.GetControlByIdAsync(created.Id, 1);
        Assert.NotNull(fetched);
        Assert.Equal(newCtrl.Code, fetched.Code);

        // 4. Update
        fetched.Title = "Updated Control Title in DB";
        fetched.Status = ControlStatus.Implemented;
        var updated = await repo.UpdateControlAsync(fetched, 1);
        Assert.NotNull(updated);
        Assert.Equal("Updated Control Title in DB", updated.Title);

        // 5. Delete
        var deleted = await repo.DeleteControlAsync(created.Id, 1);
        Assert.True(deleted);
        _output.WriteLine("Deleted test control from DB.");
    }

    [Fact]
    public async Task RiskRepository_DatabaseCrud_Operations_Succeed()
    {
        var repo = new RiskRepository(_dbFactory);

        // 1. Read
        var all = await repo.GetAllRisksAsync(1);
        Assert.NotEmpty(all);
        _output.WriteLine($"Total risks in DB: {all.Count}");

        // 2. Create
        var newRisk = new Risk
        {
            OrganizationId = 1,
            Code = $"RISK-T-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Title = "Database CRUD Test Risk",
            Description = "Risk created to test DB persistence",
            Asset = "Database Server",
            Department = "IT Operations",
            Owner = "DevSecOps",
            Likelihood = RiskLikelihood.Possible,
            Impact = RiskImpact.Major,
            Status = "Open"
        };
        var created = await repo.CreateRiskAsync(newRisk);
        Assert.NotNull(created.Id);
        _output.WriteLine($"Created risk in DB with ID: {created.Id}, Code: {created.Code}");

        // 3. Read by ID
        var fetched = await repo.GetRiskByIdAsync(created.Id, 1);
        Assert.NotNull(fetched);
        Assert.Equal(newRisk.Code, fetched.Code);

        // 4. Delete
        var deleted = await repo.DeleteRiskAsync(created.Id, 1);
        Assert.True(deleted);
        _output.WriteLine("Deleted test risk from DB.");
    }

    [Fact]
    public async Task TaskRepository_DatabaseCrud_Operations_Succeed()
    {
        var repo = new TaskRepository(_dbFactory);

        // 1. Read
        var all = await repo.GetAllTasksAsync(1);
        Assert.NotEmpty(all);
        _output.WriteLine($"Total tasks in DB: {all.Count}");

        // 2. Create
        var newTask = new TaskItem
        {
            OrganizationId = 1,
            Code = $"TASK-T-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Title = "Database CRUD Test Task",
            Owner = "Task Runner",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = ComplianceTaskStatus.NotStarted,
            Comments = "Testing DB"
        };
        var created = await repo.CreateTaskAsync(newTask);
        Assert.NotNull(created.Id);

        // 3. Update Status
        var statusUpdated = await repo.UpdateTaskStatusAsync(created.Id, ComplianceTaskStatus.Completed, 1);
        Assert.True(statusUpdated);

        // 4. Delete
        var deleted = await repo.DeleteTaskAsync(created.Id, 1);
        Assert.True(deleted);
        _output.WriteLine("Deleted test task from DB.");
    }

    [Fact]
    public async Task TraceabilityRepository_QueriesDatabaseDynamically()
    {
        var repo = new TraceabilityRepository(_dbFactory);
        var graph = await repo.GetTraceabilityGraphAsync("CTRL-001", 1);

        Assert.NotNull(graph);
        Assert.Equal(14, graph.Steps.Count);
        Assert.Contains(graph.Steps, s => s.Code.Contains("27001"));
        Assert.Contains(graph.Steps, s => s.Code == "CTRL-001");
        Assert.Contains(graph.Steps, s => s.Code == "RISK-001");

        _output.WriteLine("Traceability chain successfully loaded 14 steps dynamically from PostgreSQL database!");
    }

    [Fact]
    public async Task DashboardService_CalculatesMetricsFromDatabase()
    {
        var controlRepo = new ControlRepository(_dbFactory);
        var riskRepo = new RiskRepository(_dbFactory);
        var auditRepo = new AuditRepository(_dbFactory);
        var capaRepo = new CapaRepository(_dbFactory);
        var taskRepo = new TaskRepository(_dbFactory);
        var evidenceRepo = new EvidenceRepository(_dbFactory);
        var traceRepo = new TraceabilityRepository(_dbFactory);
        var cache = new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

        var service = new DashboardService(
            controlRepo, riskRepo, auditRepo, capaRepo,
            taskRepo, evidenceRepo, traceRepo, cache);

        var kpis = await service.GetDashboardKpisAsync(1);

        Assert.NotNull(kpis);
        Assert.True(kpis.ControlsTotalCount > 0);
        Assert.True(kpis.OverallCompliancePercentage > 0);
        _output.WriteLine($"Dashboard dynamically computed from DB: {kpis.ControlsTotalCount} Controls, {kpis.OverallCompliancePercentage}% Average Compliance, {kpis.OpenRisksCount} Open Risks.");
    }
}
