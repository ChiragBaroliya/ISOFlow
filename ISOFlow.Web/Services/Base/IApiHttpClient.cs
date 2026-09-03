namespace ISOFlow.Web.Services.Base;

/// <summary>
/// Core HTTP client interface for executing requests against ISOFlow.Api.
/// Handles JWT token propagation from Session and deserializes ApiResponse wrappers.
/// </summary>
public interface IApiHttpClient
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object? body = null);
    Task<bool> PostBoolAsync(string endpoint, object? body = null);
    Task<T?> PutAsync<T>(string endpoint, object? body = null);
    Task<bool> PutBoolAsync(string endpoint, object? body = null);
    Task<bool> DeleteAsync(string endpoint);
    Task<bool> PatchBoolAsync(string endpoint, object? body = null);
}
