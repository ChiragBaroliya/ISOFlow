using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ISOFlow.Web.Services.Base;

public class ApiHttpClient : IApiHttpClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiHttpClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private void AttachAuthorizationHeader()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var token = session?.GetString("ApiToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private StringContent ToJsonContent(object? obj) =>
        new(obj == null ? "{}" : JsonSerializer.Serialize(obj, _jsonOptions), Encoding.UTF8, "application/json");

    private async Task<T?> ExtractData<T>(HttpResponseMessage response)
    {
        var rawJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(rawJson)) return default;

        try
        {
            var wrapped = JsonSerializer.Deserialize<ApiResponseWrapper<T>>(rawJson, _jsonOptions);
            return wrapped != null && wrapped.Success ? wrapped.Data : default;
        }
        catch
        {
            // Fallback to direct deserialization if not standard envelope
            return JsonSerializer.Deserialize<T>(rawJson, _jsonOptions);
        }
    }

    private async Task<bool> ExtractSuccess(HttpResponseMessage response)
    {
        var rawJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(rawJson)) return response.IsSuccessStatusCode;

        try
        {
            var wrapped = JsonSerializer.Deserialize<ApiResponseWrapper<object>>(rawJson, _jsonOptions);
            return wrapped?.Success ?? response.IsSuccessStatusCode;
        }
        catch
        {
            return response.IsSuccessStatusCode;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AttachAuthorizationHeader();
        var response = await _http.GetAsync(endpoint);
        return await ExtractData<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? body = null)
    {
        AttachAuthorizationHeader();
        var response = await _http.PostAsync(endpoint, ToJsonContent(body));
        return await ExtractData<T>(response);
    }

    public async Task<bool> PostBoolAsync(string endpoint, object? body = null)
    {
        AttachAuthorizationHeader();
        var response = await _http.PostAsync(endpoint, ToJsonContent(body));
        return await ExtractSuccess(response);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? body = null)
    {
        AttachAuthorizationHeader();
        var response = await _http.PutAsync(endpoint, ToJsonContent(body));
        return await ExtractData<T>(response);
    }

    public async Task<bool> PutBoolAsync(string endpoint, object? body = null)
    {
        AttachAuthorizationHeader();
        var response = await _http.PutAsync(endpoint, ToJsonContent(body));
        return await ExtractSuccess(response);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        AttachAuthorizationHeader();
        var response = await _http.DeleteAsync(endpoint);
        return await ExtractSuccess(response);
    }

    public async Task<bool> PatchBoolAsync(string endpoint, object? body = null)
    {
        AttachAuthorizationHeader();
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
        if (body != null)
        {
            request.Content = ToJsonContent(body);
        }
        var response = await _http.SendAsync(request);
        return await ExtractSuccess(response);
    }
}

internal class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
