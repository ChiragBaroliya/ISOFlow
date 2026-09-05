using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

public interface ITraceabilityRepository
{
    Task<TraceabilityGraphDto> GetTraceabilityGraphAsync(string rootEntityId, int? organizationId);
}
