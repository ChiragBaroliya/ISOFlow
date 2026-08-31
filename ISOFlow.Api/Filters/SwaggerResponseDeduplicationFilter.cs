using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ISOFlow.Api.Filters;

/// <summary>
/// Swagger Operation Filter to ensure a single distinct response entry per HTTP status code (eliminates duplicate 200 status codes).
/// </summary>
public class SwaggerResponseDeduplicationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null || operation.Responses.Count <= 1)
            return;

        var uniqueResponses = new OpenApiResponses();

        foreach (var response in operation.Responses)
        {
            if (!uniqueResponses.ContainsKey(response.Key))
            {
                // Clean up duplicate media types to keep only application/json for clean documentation
                if (response.Value.Content != null && response.Value.Content.Count > 1)
                {
                    if (response.Value.Content.TryGetValue("application/json", out var jsonMediaType))
                    {
                        response.Value.Content.Clear();
                        response.Value.Content["application/json"] = jsonMediaType;
                    }
                }

                uniqueResponses.Add(response.Key, response.Value);
            }
        }

        operation.Responses = uniqueResponses;
    }
}
