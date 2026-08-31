namespace ISOFlow.Api.Controllers;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResponse(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> FailureResponse(string message, IEnumerable<string>? errors = null)
    {
        var response = new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default
        };
        if (errors != null)
        {
            response.Errors.AddRange(errors);
        }
        return response;
    }
}
