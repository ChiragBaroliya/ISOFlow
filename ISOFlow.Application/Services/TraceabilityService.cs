using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;

namespace ISOFlow.Application.Services;

public class TraceabilityService : ITraceabilityService
{
    private readonly ITraceabilityRepository _traceabilityRepository;
    private readonly ICacheService _cacheService;

    public TraceabilityService(ITraceabilityRepository traceabilityRepository, ICacheService cacheService)
    {
        _traceabilityRepository = traceabilityRepository;
        _cacheService = cacheService;
    }

    public async Task<TraceabilityGraphDto> GetTraceabilityChainAsync(string entityId)
    {
        string cacheKey = $"TraceabilityChain_{entityId}";
        var cached = _cacheService.Get<TraceabilityGraphDto>(cacheKey);
        if (cached != null) return cached;

        var graph = await _traceabilityRepository.GetTraceabilityGraphAsync(entityId);
        _cacheService.Set(cacheKey, graph, TimeSpan.FromMinutes(10));
        return graph;
    }
}
