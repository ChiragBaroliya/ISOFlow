using System.Net;
using System.Text.Json;
using ISOFlow.Api.Controllers;
using ISOFlow.Domain.Exceptions;

namespace ISOFlow.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception caught by GlobalExceptionMiddleware: {Message} | Path: {Path}", ex.Message, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";
        var errors = new List<string>();

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = validationEx.Message;
                errors.AddRange(validationEx.Errors);
                break;

            case BusinessRuleException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                message = businessEx.Message;
                errors.Add(businessEx.Message);
                break;

            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                message = argEx.Message;
                errors.Add(argEx.Message);
                break;

            case UnauthorizedAccessException authEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = authEx.Message;
                break;

            case KeyNotFoundException knfEx:
                statusCode = HttpStatusCode.NotFound;
                message = knfEx.Message;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An internal server error occurred.";
                errors.Add(exception.Message);
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Data = new
            {
                Path = context.Request.Path.Value,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            }
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
