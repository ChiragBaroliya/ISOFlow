using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Interfaces;

public interface ILogService
{
    Task<List<LogEntryDto>> GetLogsAsync(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search);
}
