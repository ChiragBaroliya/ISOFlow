using ISOFlow.Application.Services;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ISOFlow.Tests;

public class ComplianceTraceabilityTests
{
    [Fact]
    public async Task GetGoldenScenarioTraceability_ShouldReturn14StepsChain()
    {
        // Arrange
        var traceRepo = new TraceabilityRepository();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheService = new MemoryCacheService(memoryCache);
        var service = new TraceabilityService(traceRepo, cacheService);

        // Act
        var result = await service.GetTraceabilityChainAsync("CTRL-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(14, result.Steps.Count);
        Assert.Contains(result.Steps, s => s.Code == "ISO 27001:2022");
        Assert.Contains(result.Steps, s => s.Code == "A.5.18");
        Assert.Contains(result.Steps, s => s.Code == "CTRL-001");
        Assert.Contains(result.Steps, s => s.Code == "RISK-001");
        Assert.Contains(result.Steps, s => s.Code == "TRT-001");
        Assert.Contains(result.Steps, s => s.Code == "POL-001");
        Assert.Contains(result.Steps, s => s.Code == "PROC-001");
        Assert.Contains(result.Steps, s => s.Code == "TASK-2026-003");
        Assert.Contains(result.Steps, s => s.Code == "EVI-2026-001");
        Assert.Contains(result.Steps, s => s.Code == "AUD-2026-001");
        Assert.Contains(result.Steps, s => s.Code == "FIND-001");
        Assert.Contains(result.Steps, s => s.Code == "CAPA-001");
        Assert.Contains(result.Steps, s => s.Code == "REV-2026-Q4");
        Assert.Contains(result.Steps, s => s.Code == "IMP-001");
    }

    [Fact]
    public async Task GetRelatedItemsCount_ShouldReturnExactCountsForCTRL001()
    {
        // Arrange
        var controlRepo = new ControlRepository();

        // Act
        var counts = await controlRepo.GetRelatedItemsCountAsync("CTRL-001");

        // Assert
        Assert.NotNull(counts);
        Assert.Equal(2, counts.Requirements);
        Assert.Equal(1, counts.Controls);
        Assert.Equal(2, counts.Risks);
        Assert.Equal(1, counts.Treatments);
        Assert.Equal(1, counts.Policies);
        Assert.Equal(1, counts.Processes);
        Assert.Equal(4, counts.Tasks);
        Assert.Equal(2, counts.Evidence);
        Assert.Equal(1, counts.Audits);
        Assert.Equal(1, counts.Findings);
        Assert.Equal(1, counts.Capa);
        Assert.Equal(1, counts.Improvements);
    }

    [Fact]
    public async Task StandardRepository_CreateAndPreventDeletingPreseededStandards()
    {
        // Arrange
        var repo = new StandardRepository();

        // Act - Create Custom Standard
        var custom = new Standard
        {
            Code = "SOC-2-TYPE-II",
            Name = "SOC 2 Type II Compliance Framework",
            Revision = "2026",
            CompliancePercentage = 95.0,
            RequirementCount = 20
        };
        var created = await repo.CreateStandardAsync(custom);

        // Assert Created
        Assert.NotNull(created);
        Assert.False(created.IsPreseeded);
        Assert.Equal("SOC-2-TYPE-II", created.Code);

        // Act - Attempt Deleting Preseeded Official Standard
        var deletedOfficial = await repo.DeleteStandardAsync("ISO-27001-2022");
        Assert.False(deletedOfficial); // Must be protected!

        // Act - Delete Custom Standard
        var deletedCustom = await repo.DeleteStandardAsync(created.Id);
        Assert.True(deletedCustom);
    }

    [Fact]
    public async Task DocumentRepository_CreateAndManageProcesses()
    {
        // Arrange
        var repo = new DocumentRepository();

        // Act - Fetch Pre-seeded processes
        var initialProcesses = await repo.GetAllProcessesAsync();
        Assert.NotEmpty(initialProcesses);

        // Act - Create New Process
        var newProcess = new Process
        {
            Code = "PROC-TEST-001",
            Title = "Change Management Procedure",
            Category = "IT Operations",
            Owner = "Chirag Baroliya",
            Steps = new List<string> { "1. Ticket Created", "2. Peer Review", "3. Deploy to Prod" }
        };
        var created = await repo.CreateProcessAsync(newProcess);

        // Assert
        Assert.NotNull(created);
        Assert.Equal("PROC-TEST-001", created.Code);
        Assert.Equal(3, created.Steps.Count);

        // Act - Update Process
        created.Title = "Updated Change Management Procedure";
        var updated = await repo.UpdateProcessAsync(created);
        Assert.NotNull(updated);
        Assert.Equal("Updated Change Management Procedure", updated.Title);

        // Act - Archive Process
        var archived = await repo.ArchiveProcessAsync(created.Id);
        Assert.True(archived);
        var fetched = await repo.GetProcessByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Archived", fetched.Status);
    }

    [Fact]
    public async Task OrganizationRepository_CRUD_And_UserIsolation_Tests()
    {
        // Arrange
        var orgRepo = new OrganizationRepository();
        var userRepo = new UserRepository();

        // Act - Fetch preseeded organizations
        var orgs = await orgRepo.GetAllOrganizationsAsync();
        Assert.True(orgs.Count >= 3);
        Assert.Contains(orgs, o => o.Code == "ACME");
        Assert.Contains(orgs, o => o.Code == "CYBER");
        Assert.Contains(orgs, o => o.Code == "NEXUS");

        // Act - Create New Organization Tenant
        var newOrg = new Organization
        {
            Code = "APEX",
            Name = "Apex Logistics Solutions",
            Industry = "Logistics & Supply Chain",
            Employees = 500,
            PrimaryStandard = "ISO 27001:2022",
            CompliancePercentage = 88.0,
            ContactEmail = "info@apexlogistics.com"
        };
        var createdOrg = await orgRepo.CreateOrganizationAsync(newOrg);

        // Assert Created
        Assert.NotNull(createdOrg);
        Assert.Equal("APEX", createdOrg.Code);

        // Act - Test User Isolation by Organization ID
        var acmeUsers = await userRepo.GetUsersByOrganizationIdAsync("ORG-001");
        var cyberUsers = await userRepo.GetUsersByOrganizationIdAsync("ORG-002");

        Assert.NotEmpty(acmeUsers);
        Assert.NotEmpty(cyberUsers);
        Assert.All(acmeUsers, u => Assert.Equal("ORG-001", u.OrganizationId));
        Assert.All(cyberUsers, u => Assert.Equal("ORG-002", u.OrganizationId));

        // Act - Delete Created Organization
        var deleted = await orgRepo.DeleteOrganizationAsync(createdOrg.Id);
        Assert.True(deleted);
    }

    [Fact]
    public async Task LogService_DateAndLevelFiltering_Tests()
    {
        // Arrange - Create temp log directory
        var tempPath = Path.Combine(Path.GetTempPath(), "ISOFlowTestLogs_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempPath);

        try
        {
            var todayStr = DateTime.Today.ToString("yyyyMMdd");
            var yesterdayStr = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

            var logFileToday = Path.Combine(tempPath, $"isoflow-{todayStr}.log");
            var logFileYesterday = Path.Combine(tempPath, $"isoflow-{yesterdayStr}.log");

            File.WriteAllText(logFileToday, $"{DateTime.Today:yyyy-MM-dd} 10:00:00.000 +00:00 [INF] Today Info Log Message\n{DateTime.Today:yyyy-MM-dd} 11:00:00.000 +00:00 [ERR] Today Error Exception\n");
            File.WriteAllText(logFileYesterday, $"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 15:00:00.000 +00:00 [WRN] Yesterday Warning Log Message\n");

            var service = new LogService(tempPath);

            // Act 1: Fetch logs for Today only
            var logsToday = await service.GetLogsAsync(DateTime.Today, DateTime.Today, "All", null);
            Assert.Equal(2, logsToday.Count);

            // Act 2: Fetch Errors only
            var errorLogs = await service.GetLogsAsync(null, null, "Error", null);
            Assert.Single(errorLogs);
            Assert.Equal("Error", errorLogs[0].Level);
            Assert.Contains("Today Error Exception", errorLogs[0].Message);

            // Act 3: Filter by date range (Yesterday to Today)
            var allLogs = await service.GetLogsAsync(DateTime.Today.AddDays(-1), DateTime.Today, "All", null);
            Assert.Equal(3, allLogs.Count);
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }
}
