namespace ISOFlow.Application.DTOs;

public class LogEntryDto
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "Information";
    public string SourceContext { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string RawLine { get; set; } = string.Empty;
}

public class LogFilterQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? LogLevel { get; set; }
    public string? Search { get; set; }
}
