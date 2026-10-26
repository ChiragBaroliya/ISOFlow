using ISOFlow.Application.DTOs;
using ISOFlow.Web.Services.Base;

namespace ISOFlow.Web.Services.Traceability;

public interface ITraceabilityApiClient
{
    /// <summary>
    /// Returns the compliance chain around the given entity code (control, risk, task, evidence,
    /// finding or CAPA). An entity with no linked control yields an empty chain.
    /// </summary>
    Task<TraceabilityGraphDto> GetChainAsync(string entityCode);
}

public class TraceabilityApiClient : ITraceabilityApiClient
{
    private readonly IApiHttpClient _api;

    public TraceabilityApiClient(IApiHttpClient api)
    {
        _api = api;
    }

    public async Task<TraceabilityGraphDto> GetChainAsync(string entityCode) =>
        await _api.GetAsync<TraceabilityGraphDto>($"api/traceability/{Uri.EscapeDataString(entityCode)}")
            ?? new TraceabilityGraphDto { RootId = entityCode };
}
