using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ISOFlow.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ISOFlow.Web.Services.Base;

public class ApiHttpClient : IApiHttpClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiHttpClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public ApiHttpClient(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<ApiHttpClient> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
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

    private async Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> call, string endpoint)
    {
        AttachAuthorizationHeader();
        try
        {
            var response = await call();
            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            {
                return response;
            }

            // The 15-minute access token has likely expired. Try to silently renew it from the
            // refresh token stored at login, then retry this request once, before giving up and
            // letting the caller's normal 401 handling (session clear + redirect to Login) run.
            var expiredToken = _http.DefaultRequestHeaders.Authorization?.Parameter;
            if (string.IsNullOrEmpty(expiredToken) || !await TryRefreshTokenAsync(expiredToken))
            {
                return response;
            }

            response.Dispose();
            AttachAuthorizationHeader();
            return await call();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API Connection Failure: Unable to reach ISOFlow.Api at {BaseAddress}{Endpoint}. Ensure ISOFlow.Api is running.", _http.BaseAddress, endpoint);
            throw new HttpRequestException(
                $"[ISOFlow Backend Offline] Unable to reach backend API at '{_http.BaseAddress}'. " +
                "Please verify that 'ISOFlow.Api' is running (listening on port 5015).\n\n" +
                "To fix in Visual Studio:\n" +
                "1. Look at the Startup dropdown next to the green Play button.\n" +
                "2. Select 'ISOFlow Full Platform (API + Web)' or configure 'Multiple startup projects'.\n" +
                "3. Press F5 / Ctrl+F5 so both projects launch simultaneously.\n\n" +
                $"Details: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Attempts to renew the access token using the refresh token stored at login. Serialized via a
    /// process-wide lock: refresh tokens rotate on use (old one is revoked), so two requests racing
    /// to refresh with the same stale token would otherwise trigger the API's reuse-detection and
    /// revoke the whole session. Safe to call from multiple concurrent requests — once one succeeds,
    /// the rest observe the already-updated session token and skip calling the API again.
    /// </summary>
    private async Task<bool> TryRefreshTokenAsync(string expiredToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        await RefreshLock.WaitAsync();
        try
        {
            var currentToken = httpContext.Session.GetString("ApiToken");
            if (!string.IsNullOrWhiteSpace(currentToken) && currentToken != expiredToken)
            {
                return true; // another concurrent request already refreshed while we were waiting
            }

            var refreshToken = httpContext.Session.GetString("RefreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken)) return false;

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync("api/auth/refresh", ToJsonContent(new RefreshTokenRequestDto { RefreshToken = refreshToken }));
            }
            catch (HttpRequestException)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode) return false;

            var raw = await response.Content.ReadAsStringAsync();
            ApiResponseWrapper<TokenResponseDto>? wrapped;
            try
            {
                wrapped = JsonSerializer.Deserialize<ApiResponseWrapper<TokenResponseDto>>(raw, _jsonOptions);
            }
            catch (JsonException)
            {
                return false;
            }

            if (wrapped is not { Success: true, Data: not null })
            {
                return false;
            }

            httpContext.Session.SetString("ApiToken", wrapped.Data.AccessToken);
            httpContext.Session.SetString("RefreshToken", wrapped.Data.RefreshToken);
            return true;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private StringContent ToJsonContent(object? obj) =>
        new(obj == null ? "{}" : JsonSerializer.Serialize(obj, _jsonOptions), Encoding.UTF8, "application/json");

    private void HandleUnauthorized(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.Clear();
                if (!httpContext.Response.HasStarted &&
                    !httpContext.Request.Path.StartsWithSegments("/Account/Login"))
                {
                    httpContext.Response.Redirect("/Account/Login?sessionExpired=true");
                }
            }
        }
    }

    private async Task<T?> ExtractData<T>(HttpResponseMessage response)
    {
        HandleUnauthorized(response);
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
        HandleUnauthorized(response);
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
        var response = await SendRequestAsync(() => _http.GetAsync(endpoint), endpoint);
        return await ExtractData<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? body = null)
    {
        var response = await SendRequestAsync(() => _http.PostAsync(endpoint, ToJsonContent(body)), endpoint);
        return await ExtractData<T>(response);
    }

    public async Task<bool> PostBoolAsync(string endpoint, object? body = null)
    {
        var response = await SendRequestAsync(() => _http.PostAsync(endpoint, ToJsonContent(body)), endpoint);
        return await ExtractSuccess(response);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? body = null)
    {
        var response = await SendRequestAsync(() => _http.PutAsync(endpoint, ToJsonContent(body)), endpoint);
        return await ExtractData<T>(response);
    }

    public async Task<bool> PutBoolAsync(string endpoint, object? body = null)
    {
        var response = await SendRequestAsync(() => _http.PutAsync(endpoint, ToJsonContent(body)), endpoint);
        return await ExtractSuccess(response);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        var response = await SendRequestAsync(() => _http.DeleteAsync(endpoint), endpoint);
        return await ExtractSuccess(response);
    }

    public async Task<bool> PatchBoolAsync(string endpoint, object? body = null)
    {
        var response = await SendRequestAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
            if (body != null)
            {
                request.Content = ToJsonContent(body);
            }
            return _http.SendAsync(request);
        }, endpoint);
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
