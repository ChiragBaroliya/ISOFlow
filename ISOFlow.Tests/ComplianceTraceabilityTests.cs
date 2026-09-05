using ISOFlow.Application.Services;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using ISOFlow.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ISOFlow.Tests;

public class ComplianceTraceabilityTests
{
    private readonly IDbConnectionFactory _dbFactory = new DbConnectionFactory("Host=localhost;Database=isoflow;Username=postgres;Password=postgres;");

    [Fact]
    public async Task GetGoldenScenarioTraceability_ShouldReturn14StepsChain()
    {
        // Arrange
        var traceRepo = new TraceabilityRepository(_dbFactory);
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheService = new MemoryCacheService(memoryCache);
        var service = new TraceabilityService(traceRepo, cacheService);

        // Act
        var result = await service.GetTraceabilityChainAsync("CTRL-001", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(14, result.Steps.Count);
        Assert.Contains(result.Steps, s => s.Code.Contains("27001"));
        Assert.Contains(result.Steps, s => s.Code == "A.5.18");
        Assert.Contains(result.Steps, s => s.Code == "CTRL-001");
        Assert.Contains(result.Steps, s => s.Code == "RISK-001");
        Assert.Contains(result.Steps, s => s.Code == "TRT-001");
        Assert.Contains(result.Steps, s => s.Code == "POL-001");
        Assert.Contains(result.Steps, s => s.Code == "PROC-001");
        Assert.Contains(result.Steps, s => s.Code.StartsWith("TASK-"));
        Assert.Contains(result.Steps, s => s.Code == "EVI-2026-001");
        Assert.Contains(result.Steps, s => s.Code == "AUD-2026-001");
        Assert.Contains(result.Steps, s => s.Code == "FIND-001");
        Assert.Contains(result.Steps, s => s.Code == "CAPA-001");
        Assert.Contains(result.Steps, s => s.Code.Contains("Q4"));
        Assert.Contains(result.Steps, s => s.Code == "IMP-001");
    }

    [Fact]
    public void StandardEntity_Initialization_Tests()
    {
        var standard = new Standard
        {
            Code = "ISO-27001-2022",
            Name = "ISO/IEC 27001:2022 Information Security Management",
            Revision = "2022",
            RequirementCount = 93,
            CompliancePercentage = 78.5,
            IsPreseeded = true,
            Status = "Active"
        };

        Assert.Equal("ISO-27001-2022", standard.Code);
        Assert.True(standard.IsPreseeded);
        Assert.Equal(93, standard.RequirementCount);
    }

    [Fact]
    public void ControlEntity_StatusAndApplicability_Tests()
    {
        var control = new Control
        {
            Code = "CTRL-001",
            Title = "User Access Management",
            Status = ControlStatus.Implemented,
            Owner = "Security Team",
            CompliancePercentage = 100.0,
            IsApplicable = true,
            Justification = "Critical security control"
        };

        Assert.Equal("CTRL-001", control.Code);
        Assert.Equal(ControlStatus.Implemented, control.Status);
        Assert.True(control.IsApplicable);
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
