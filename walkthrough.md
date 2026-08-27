# Walkthrough — ISOFlow: Global Error Management, Serilog Rolling Logs, Log Filtering & Swagger API

We have implemented **Global Error Management**, **Date-Wise Rolling Daily Logging with Serilog**, **Log Management with From-Date/To-Date Filters**, and **Swagger OpenAPI Documentation** across **ISOFlow.Api** and **ISOFlow.Web**.

---

## 1. Key Accomplishments

### ⚠️ 1. Global Error & Exception Management
- **Middleware Interceptor**: Built [GlobalExceptionMiddleware.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Middlewares/GlobalExceptionMiddleware.cs) to catch unhandled application exceptions.
- **Standardized Error Payload**: Automatically outputs standardized JSON error objects with `Success = false`, `Message`, `TraceId`, `Path`, and `Timestamp` (HTTP 500, 404, 400).
- **Structured Error Logging**: Captures full exception stack traces to Serilog logs before returning clean, secure user-facing responses.

---

### 📝 2. Serilog Date-Wise Daily Rolling Log Engine
- **Date-Stamped Rolling Files**: Configured Serilog rolling file sinks in both [ISOFlow.Api/Program.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Program.cs) and [ISOFlow.Web/Program.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Program.cs) targeting `logs/isoflow-yyyyMMdd.log` with `RollingInterval.Day`.
- **Log Formatting**: Formatted log lines with ISO timestamps, log severity level (`[INF]`, `[WRN]`, `[ERR]`), source context, and multiline exception stack traces.

---

### 🔍 3. Log Management Hub & Date Range Filtering (`/Logs`)
- **Log Service**: Implemented [LogService.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Infrastructure/Services/LogService.cs) to parse daily rolling log files safely using read-share locks.
- **Filter Criteria Supported**:
  - **From Date** (`fromDate`) and **To Date** (`toDate`) datetime range filter.
  - **Log Level** filter (`All`, `Information`, `Warning`, `Error`, `Fatal`, `Debug`).
  - **Search Keyword** query matching messages or exception traces.
- **Interactive UI & API**:
  - Web UI: Created [Index.cshtml](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Views/Logs/Index.cshtml) with date range pickers, level selector pills, summary counters, level badges, and collapsible stack trace drawers.
  - REST API: Created [LogsApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/LogsApiController.cs) exposing `GET /api/logs` with query parameters.

---

### 📜 4. Swagger & OpenAPI Documentation in `ISOFlow.Api`
- **Swagger UI Integration**: Integrated `Swashbuckle.AspNetCore` at `/swagger` with metadata, API descriptions, and XML documentation comments.
- **Modular API Controllers**: Split `ApiControllers.cs` into individual controller files:
  - [DashboardApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/DashboardApiController.cs)
  - [StandardsApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/StandardsApiController.cs)
  - [ControlsApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/ControlsApiController.cs)
  - [RisksApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/RisksApiController.cs)
  - [TraceabilityApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/TraceabilityApiController.cs)
  - [LogsApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/LogsApiController.cs)

---

## 2. Verification Results

### Automated xUnit Unit Tests
Executed `dotnet test ISOFlow.slnx` verifying 6 passing unit test suites:
```text
Test run for D:\LearningProject\ISOFlow\ISOFlow.Tests\bin\Debug\net10.0\ISOFlow.Tests.dll (.NETCoreApp,Version=v10.0)
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6, Duration: 44 ms
```
- Verified `LogService_DateAndLevelFiltering_Tests`: tested log file parsing, date range filtering (`fromDate` / `toDate`), log level filtering, and keyword search.
