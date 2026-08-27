using System.Globalization;
using System.Text.RegularExpressions;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;

namespace ISOFlow.Infrastructure.Services;

public class LogService : ILogService
{
    private readonly string _logsDirectory;

    public LogService(string? customLogsDirectory = null)
    {
        _logsDirectory = customLogsDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "logs");
        if (!Directory.Exists(_logsDirectory))
        {
            Directory.CreateDirectory(_logsDirectory);
        }
    }

    public async Task<List<LogEntryDto>> GetLogsAsync(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search)
    {
        var entries = new List<LogEntryDto>();

        if (!Directory.Exists(_logsDirectory))
        {
            return entries;
        }

        var logFiles = Directory.GetFiles(_logsDirectory, "*.log")
                               .Concat(Directory.GetFiles(_logsDirectory, "*.txt"))
                               .ToList();

        // Extract dates from filenames (e.g. isoflow-20260812.log)
        var datePattern = new Regex(@"\d{8}");

        foreach (var file in logFiles)
        {
            var fileName = Path.GetFileName(file);
            DateTime fileDate = File.GetLastWriteTime(file).Date;

            var match = datePattern.Match(fileName);
            if (match.Success && DateTime.TryParseExact(match.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                fileDate = parsedDate.Date;
            }

            // Date filtering on file level
            if (fromDate.HasValue && fileDate < fromDate.Value.Date)
            {
                continue;
            }
            if (toDate.HasValue && fileDate > toDate.Value.Date)
            {
                continue;
            }

            try
            {
                // Read lines safely even if locked by Serilog sink using FileShare.ReadWrite
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                
                string? line;
                LogEntryDto? currentEntry = null;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Serilog output format check: "2026-08-12 13:45:00.123 +05:30 [INF] Message..."
                    var entryMatch = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:\s[+-]\d{2}:\d{2})?)\s+\[(\w+)\]\s+(.*)$");

                    if (entryMatch.Success)
                    {
                        if (currentEntry != null)
                        {
                            entries.Add(currentEntry);
                        }

                        var timeStr = entryMatch.Groups[1].Value.Trim();
                        var levelStr = entryMatch.Groups[2].Value.Trim();
                        var msgStr = entryMatch.Groups[3].Value.Trim();

                        DateTime parsedTimestamp = fileDate;
                        if (DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            parsedTimestamp = dt;
                        }

                        currentEntry = new LogEntryDto
                        {
                            Timestamp = parsedTimestamp,
                            Level = NormalizeLogLevel(levelStr),
                            Message = msgStr,
                            RawLine = line
                        };
                    }
                    else if (currentEntry != null)
                    {
                        // Multiline exception stack trace
                        if (string.IsNullOrEmpty(currentEntry.Exception))
                        {
                            currentEntry.Exception = line;
                        }
                        else
                        {
                            currentEntry.Exception += Environment.NewLine + line;
                        }
                    }
                }

                if (currentEntry != null)
                {
                    entries.Add(currentEntry);
                }
            }
            catch
            {
                // Ignore read errors for active file lock fallback
            }
        }

        // Apply in-memory Filters
        var query = entries.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp.Date >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp.Date <= toDate.Value.Date);
        }
        if (!string.IsNullOrWhiteSpace(logLevel) && !logLevel.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.Level.Equals(logLevel, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(e => (e.Message != null && e.Message.ToLowerInvariant().Contains(term)) ||
                                     (e.Exception != null && e.Exception.ToLowerInvariant().Contains(term)));
        }

        return query.OrderByDescending(e => e.Timestamp).ToList();
    }

    private static string NormalizeLogLevel(string shortLevel)
    {
        return shortLevel.ToUpperInvariant() switch
        {
            "INF" or "INFORMATION" => "Information",
            "WRN" or "WARNING" or "WARN" => "Warning",
            "ERR" or "ERROR" => "Error",
            "FTL" or "FATAL" => "Fatal",
            "DBG" or "DEBUG" => "Debug",
            "VRB" or "VERBOSE" or "TRACE" => "Verbose",
            _ => shortLevel
        };
    }
}
