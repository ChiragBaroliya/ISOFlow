using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

public interface ITraceabilityService
{
    Task<TraceabilityGraphDto> GetTraceabilityChainAsync(string entityId, int? organizationId);
}
