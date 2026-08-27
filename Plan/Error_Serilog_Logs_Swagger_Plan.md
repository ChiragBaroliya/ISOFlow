# Implementation Plan — Error Management, Serilog Rolling Logs, Log Management UI & Filter, and Swagger API Documentation

Implement enterprise-grade **Error Management**, **Daily Rolling File Logging with Serilog**, **Interactive Log Management UI with From-Date/To-Date Filters**, and **Enhanced Swagger OpenAPI Documentation** across **ISOFlow.Api** and **ISOFlow.Web**.

---

## Technical Architecture & Components

> [!IMPORTANT]
> **1. Global Error & Exception Management**:
> - **Global Exception Handling Middleware**: Intercepts unhandled exceptions across Web API and MVC controllers.
> - **Standardized Error Responses**: Formats API errors into standardized `ApiResponse<T>` / `ProblemDetails` with HTTP Status (500, 404, 400), Timestamp, TraceId, and user-friendly error messages while logging full stack traces.
> 
> **2. Serilog Daily File Logging**:
> - **Rolling File Sink**: Configure Serilog to generate date-wise log files (`logs/isoflow-yyyyMMdd.log` with `RollingInterval.Day`).
> - **Structured Logging**: Log HTTP requests, unhandled errors, user actions, and system diagnostics with structured properties (`Timestamp`, `Level`, `SourceContext`, `Message`, `Exception`).
> 
> **3. Log Management Hub & Date Filtering (`/Logs`)**:
> - **Log Service (`ILogService`)**: Parses Serilog rolling log files from the `logs/` directory.
> - **Filter Criteria**:
>   - `FromDate` & `ToDate` datetime range filter.
>   - `LogLevel` filter (All, Information, Warning, Error, Fatal).
>   - `Search` keyword query.
> - **MVC View (`/Logs/Index.cshtml`)**: Interactive Log viewer dashboard featuring date range pickers, filter bar, log entry summary cards, level indicators, and expandable exception trace details.
> 
> **4. Enhanced Swagger OpenAPI Documentation in `ISOFlow.Api`**:
> - **Swagger & OpenAPI Setup**: Configure `Swashbuckle.AspNetCore` with Swagger UI at `/swagger`.
> - **XML Documentation Comments**: Enable XML documentation output in `.csproj` to enrich Swagger UI with endpoint descriptions, parameter specs, and response types.
> - **Modular API Controllers**: Split `ApiControllers.cs` into individual controller files (`DashboardApiController.cs`, `StandardsApiController.cs`, `ControlsApiController.cs`, `RisksApiController.cs`, `TraceabilityApiController.cs`, `LogsApiController.cs`).

---

## Proposed Changes

### Core Projects & Dependencies

#### [MODIFY] [ISOFlow.Api.csproj](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/ISOFlow.Api.csproj) & [ISOFlow.Web.csproj](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/ISOFlow.Web.csproj)
- Add NuGet packages:
  - `Serilog.AspNetCore` (v9.0.0+)
  - `Serilog.Sinks.File` (v6.0.0+)
  - `Serilog.Sinks.Console` (v6.0.0+)

---

### Application & Infrastructure Layers

#### [NEW] [ILogService.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Application/Interfaces/ILogService.cs)
- Define `ILogService`:
  - `Task<List<LogEntryDto>> GetLogsAsync(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search);`

#### [NEW] [LogService.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Infrastructure/Services/LogService.cs)
- Implement `LogService`: Parses `logs/isoflow-yyyyMMdd.log` files, filters entries between `FromDate` and `ToDate`, filters by `LogLevel` and `Search` terms, and returns DTO models.

#### [NEW] [GlobalExceptionMiddleware.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Middlewares/GlobalExceptionMiddleware.cs) & [ISOFlow.Web Exception Handler]
- Global middleware intercepting errors, writing structured Serilog Error events, and returning consistent HTTP error payloads.

---

### MVC Web & API Controllers

#### [NEW] [LogsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/LogsController.cs)
- `Index(DateTime? fromDate, DateTime? toDate, string? logLevel, string? search)`: Serves the Log Management UI with filtered logs.

#### [NEW] [LogsApiController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Api/Controllers/LogsApiController.cs)
- `GET /api/logs`: Returns filtered logs JSON array for API consumers.

#### [NEW] Modular API Controllers in `ISOFlow.Api/Controllers/`:
- `DashboardApiController.cs`
- `StandardsApiController.cs`
- `ControlsApiController.cs`
- `RisksApiController.cs`
- `TraceabilityApiController.cs`
- Delete monolithic `ApiControllers.cs`.

---

### Views & UI Components

#### [NEW] [Views/Logs/Index.cshtml](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Views/Logs/Index.cshtml)
- Log Management Hub UI featuring:
  - Date Range pickers (`From Date` & `To Date`).
  - Log Level selector pills.
  - Search keyword input.
  - Interactive table of log entries with level badges and stack trace drawer.

#### [MODIFY] [Views/Shared/_Layout.cshtml](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Views/Shared/_Layout.cshtml)
- Add "System Logs" link under `SYSTEM & LOGS` section in sidebar.

---

## Verification Plan

### Automated Tests
- Add unit tests in `ISOFlow.Tests` verifying log parsing, date range filtering (`fromDate`, `toDate`), and exception middleware responses.
- Run `dotnet test ISOFlow.slnx`.

### Manual & Visual Verification
1. Launch `ISOFlow.Api` on `http://localhost:5032`:
   - Open `/swagger` in browser and verify API documentation, endpoints, and models.
2. Launch `ISOFlow.Web` on `http://localhost:5198`:
   - Navigate to `/Logs`.
   - Test filtering logs by **From Date** and **To Date**.
   - Test filtering by Log Level (**Error**, **Warning**, **Information**).
   - Check `logs/` directory to verify daily file creation (`isoflow-yyyyMMdd.log`).
