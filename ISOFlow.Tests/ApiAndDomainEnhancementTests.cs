using ISOFlow.Api.Controllers;
using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Domain.Exceptions;
using ISOFlow.Domain.Helpers;
using ISOFlow.Infrastructure.Data;
using ISOFlow.Infrastructure.Repositories;
using Xunit;

namespace ISOFlow.Tests;

public class ApiAndDomainEnhancementTests
{
    private readonly IDbConnectionFactory _dbFactory = new DbConnectionFactory("Host=localhost;Database=isoflow;Username=postgres;Password=postgres;");

    [Fact]
    public void PagedResponse_CalculatesMetadataCorrectly()
    {
        var items = new List<string> { "Item1", "Item2" };
        var pagedResponse = new PagedResponse<string>(items, 15, 1, 2);

        Assert.Equal(15, pagedResponse.TotalCount);
        Assert.Equal(8, pagedResponse.TotalPages);
        Assert.Equal(1, pagedResponse.PageNumber);
        Assert.Equal(2, pagedResponse.PageSize);
        Assert.True(pagedResponse.HasNextPage);
        Assert.False(pagedResponse.HasPreviousPage);
    }

    [Fact]
    public void DomainHelpers_CalculateRiskScoreAndLevel_Accurate()
    {
        var score = RiskEvaluationHelper.CalculateScore(RiskLikelihood.Likely, RiskImpact.Major);
        Assert.Equal(16, score);

        var level = RiskEvaluationHelper.DetermineRiskLevel(score);
        Assert.Equal(RiskLevel.Critical, level);

        var reduction = RiskEvaluationHelper.CalculateRiskReductionPercentage(16, 4);
        Assert.Equal(75.0, reduction);
    }

    [Fact]
    public void DomainHelpers_ComplianceScoreCalculator_CalculatesAverage()
    {
        var reqs = new List<Requirement>
        {
            new() { CompliancePercentage = 80.0 },
            new() { CompliancePercentage = 100.0 },
            new() { CompliancePercentage = 60.0 }
        };

        var avg = ComplianceScoreCalculator.CalculateStandardCompliance(reqs);
        Assert.Equal(80.0, avg);
    }

    [Fact]
    public void DomainHelpers_CodeGenerator_FormatsCorrectly()
    {
        Assert.Equal("CTRL-005", CodeGeneratorHelper.GenerateControlCode(5));
        Assert.Equal("RISK-012", CodeGeneratorHelper.GenerateRiskCode(12));
        Assert.Equal("TASK-2026-003", CodeGeneratorHelper.GenerateTaskCode(3, 2026));
    }

    [Fact]
    public void ApiResponse_SuccessAndFailureFactories_BehaveCorrectly()
    {
        var successResponse = ApiResponse<string>.SuccessResponse("DataValue", "Operation completed.");
        Assert.True(successResponse.Success);
        Assert.Equal("DataValue", successResponse.Data);
        Assert.Empty(successResponse.Errors);

        var failureResponse = ApiResponse<string>.FailureResponse("Operation failed.", new[] { "Field 'Code' is required", "Field 'Name' is required" });
        Assert.False(failureResponse.Success);
        Assert.Null(failureResponse.Data);
        Assert.Equal(2, failureResponse.Errors.Count);
    }

    [Fact]
    public void DomainExceptions_InstantiateWithMessages()
    {
        var notFound = new NotFoundException("Standard", "STD-999");
        Assert.Contains("STD-999", notFound.Message);

        var validation = new ValidationException(new[] { "Error 1", "Error 2" });
        Assert.Equal(2, validation.Errors.Count);

        var business = new BusinessRuleException("Cannot delete official standard.");
        Assert.Equal("Cannot delete official standard.", business.Message);
    }
}
