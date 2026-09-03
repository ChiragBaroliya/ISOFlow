# Implementation Plan — ISOFlow: ISO Compliance Management Platform (MVC Architecture)

Build a fresh, modern, enterprise-grade **ISO Compliance Management Platform** called **ISOFlow** targeting .NET 10, ASP.NET Core MVC, Web API, and clean software architecture.

ISOFlow orchestrates the complete ISO compliance lifecycle through **Compliance Traceability**:
Every entity in the platform is connected and navigable in both forward and backward directions.

---

## Technical Update: API-First Architecture — ISOFlow.Web Consumes ISOFlow.Api

> [!IMPORTANT]
> **API-First Architecture Update**:
> - **ISOFlow.Web** is a **pure MVC UI client**. It contains **zero business logic** and **zero direct repository/infrastructure references**.
> - **All data operations** (CRUD, queries, authentication, logs) flow through **ISOFlow.Api** REST endpoints via modular, domain-specific typed `HttpClient` services.
> - **ISOFlow.Api** is the **single source of truth** — it owns all repositories, services, validation, pagination, JWT authentication, and data stores.
> - **Data flow**: `User → Razor View → MVC Controller → Domain ApiClient (HttpClient) → ISOFlow.Api REST Endpoint → Repository → Data Store`
> - **ISOFlow.Web references only**: `ISOFlow.Domain` (entity types for views) and `ISOFlow.Application` (DTOs and ViewModels). **No `ISOFlow.Infrastructure` reference**.
> - **Both projects must run simultaneously**: API on `http://localhost:5015`, Web on `http://localhost:5285`.

> [!IMPORTANT]
> **Web UI Architecture & Modular Service Separation**:
> - **Framework**: **ASP.NET Core MVC** (.NET 10).
> - **Modular Controller Files**: Every MVC controller resides in its own dedicated C# source file inside `ISOFlow.Web/Controllers/`.
> - **Modular Domain API Clients** (`ISOFlow.Web/Services/`):
>   Instead of a monolithic single API client, all API calls are strictly separated by domain/feature area:
>   - **`Base/`**: `IApiHttpClient.cs` & `ApiHttpClient.cs` (Centralized HTTP request execution, Session JWT token attachment, and `ApiResponse<T>` unwrapping).
>   - **`Dashboard/`**: `IDashboardApiClient.cs` & `DashboardApiClient.cs`
>   - **`Standards/`**: `IStandardsApiClient.cs` & `StandardsApiClient.cs`
>   - **`Controls/`**: `IControlsApiClient.cs` & `ControlsApiClient.cs`
>   - **`Risks/`**: `IRisksApiClient.cs` & `RisksApiClient.cs`
>   - **`Documents/`**: `IDocumentsApiClient.cs` & `DocumentsApiClient.cs`
>   - **`Tasks/`**: `ITasksApiClient.cs` & `TasksApiClient.cs`
>   - **`Evidence/`**: `IEvidenceApiClient.cs` & `EvidenceApiClient.cs`
>   - **`Audits/`**: `IAuditsApiClient.cs` & `AuditsApiClient.cs`
>   - **`Findings/`**: `IFindingsApiClient.cs` & `FindingsApiClient.cs`
>   - **`Capa/`**: `ICapaApiClient.cs` & `CapaApiClient.cs`
>   - **`ManagementReviews/`**: `IManagementReviewsApiClient.cs` & `ManagementReviewsApiClient.cs`
>   - **`Improvements/`**: `IImprovementsApiClient.cs` & `ImprovementsApiClient.cs`
>   - **`Users/`**: `IUsersApiClient.cs` & `UsersApiClient.cs`
>   - **`Organizations/`**: `IOrganizationsApiClient.cs` & `OrganizationsApiClient.cs`
>   - **`Auth/`**: `IAuthApiClient.cs` & `AuthApiClient.cs`
>   - **`Logs/`**: `ILogsApiClient.cs` & `LogsApiClient.cs`
> - **`Models/`**: Dedicated ViewModels (`DashboardViewModel.cs`, `ControlDetailViewModel.cs`, `RiskDetailViewModel.cs`, `TaskKanbanViewModel.cs`).
> - **`Views/`**: Razor `.cshtml` view templates grouped cleanly by Controller folder.
> - **`wwwroot/js/`**: Modularized client JavaScript files (`site.js`, `dashboard.js`, `kanban.js`, `signalr-client.js`).

---

## Central Product Concept & Lifecycle Flow

### 1. Master Compliance Traceability Lifecycle

```text
Standard
   ↓
Requirement
   ↓
Control
   ↓
Risk
   ↓
Risk Treatment
   ↓
Policy / Process
   ↓
Task
   ↓
Evidence
   ↓
Audit
   ↓
Finding / Non-Conformity
   ↓
CAPA
   ↓
Management Review
   ↓
Continual Improvement
```

---

### 2. Fully Clickable End-to-End Demo Scenario

```text
ISO 27001:2022
       ↓
A.5.18 Access Rights
       ↓
CTRL-001 User Access Management
       ↓
RISK-001 Unauthorized Access
       ↓
TRT-001 Implement JML
       ↓
Access Control Policy
       ↓
Joiner-Mover-Leaver Process
       ↓
Q3 User Access Review (Task)
       ↓
Access Review Evidence
       ↓
AUD-2026-001 Internal Audit
       ↓
FIND-001 Access Not Removed
       ↓
CAPA-001 Automate Access Lifecycle
       ↓
Q4 Management Review
       ↓
IMP-001 Access Lifecycle Automation
```

---

### 3. Control-Centric Experience (CTRL-001)

When a user opens **CTRL-001 (User Access Management)**, `ControlsController` calls `IControlsApiClient` to serve the 12-tab control view containing:
- **Requirement**: `A.5.18 Access Rights`
- **Risks**: `RISK-001 Unauthorized Access`, `RISK-002 Excessive Privileges`
- **Treatment**: `TRT-001 Implement JML`
- **Policy**: `Access Control Policy`
- **Process**: `Joiner-Mover-Leaver`
- **Tasks**: `Q1 Access Review`, `Q2 Access Review`, `Q3 Access Review`, `Q4 Access Review`
- **Evidence**: `25 Records` (e.g. `Access_Review_Report.pdf`, `Manager_Approval.pdf`)
- **Audit**: `AUD-2026-001 ISO 27001 Internal Audit`
- **Finding**: `FIND-001 Access Not Removed`
- **CAPA**: `CAPA-001 Automate Access Lifecycle`
- **Management Review**: `Q4 2026 Management Review`
- **Improvement**: `IMP-001 Access Lifecycle Automation`

---

## Solution & Project Structure

```text
ISOFlow.sln
│
├── ISOFlow.Domain          (Class Library - Domain Entities, Enums, Value Objects)
├── ISOFlow.Application     (Class Library - DTOs, Repository Interfaces, Application Services)
├── ISOFlow.Infrastructure  (Class Library - Relational Mock Repository Store, IMemoryCache Service)
│
├── ISOFlow.Api             (ASP.NET Core Web API - REST Controllers, ComplianceHub SignalR, Swagger, JWT Auth)
│   ├── Controllers/        (19 API Controller files — the SINGLE source of truth for all data operations)
│   ├── Hubs/               (ComplianceHub SignalR)
│   ├── Middlewares/        (GlobalExceptionMiddleware)
│   ├── Filters/            (SwaggerResponseDeduplicationFilter)
│   └── Program.cs          (Registers ALL repositories, services, JWT, Swagger, SignalR)
│
├── ISOFlow.Web             (ASP.NET Core MVC — PURE UI CLIENT, no business logic)
│   ├── Services/
│   │   ├── Base/
│   │   │   ├── IApiHttpClient.cs           ← Shared HTTP operations, token attachment, serialization
│   │   │   └── ApiHttpClient.cs
│   │   ├── Dashboard/                      ← IDashboardApiClient & DashboardApiClient
│   │   ├── Standards/                      ← IStandardsApiClient & StandardsApiClient
│   │   ├── Controls/                       ← IControlsApiClient & ControlsApiClient
│   │   ├── Risks/                          ← IRisksApiClient & RisksApiClient
│   │   ├── Documents/                      ← IDocumentsApiClient & DocumentsApiClient
│   │   ├── Tasks/                          ← ITasksApiClient & TasksApiClient
│   │   ├── Evidence/                       ← IEvidenceApiClient & EvidenceApiClient
│   │   ├── Audits/                         ← IAuditsApiClient & AuditsApiClient
│   │   ├── Findings/                       ← IFindingsApiClient & FindingsApiClient
│   │   ├── Capa/                           ← ICapaApiClient & CapaApiClient
│   │   ├── ManagementReviews/              ← IManagementReviewsApiClient & ManagementReviewsApiClient
│   │   ├── Improvements/                   ← IImprovementsApiClient & ImprovementsApiClient
│   │   ├── Users/                          ← IUsersApiClient & UsersApiClient
│   │   ├── Organizations/                  ← IOrganizationsApiClient & OrganizationsApiClient
│   │   ├── Auth/                           ← IAuthApiClient & AuthApiClient
│   │   └── Logs/                           ← ILogsApiClient & LogsApiClient
│   │
│   ├── Controllers/
│   │   ├── HomeController.cs             ← calls _dashboardClient
│   │   ├── StandardsController.cs        ← calls _standardsClient
│   │   ├── ControlsController.cs         ← calls _controlsClient
│   │   ├── RisksController.cs            ← calls _risksClient
│   │   ├── DocumentsController.cs        ← calls _documentsClient
│   │   ├── TasksController.cs            ← calls _tasksClient
│   │   ├── EvidenceController.cs         ← calls _evidenceClient
│   │   ├── AuditsController.cs           ← calls _auditsClient
│   │   ├── FindingsController.cs         ← calls _findingsClient
│   │   ├── CapaController.cs             ← calls _capaClient
│   │   ├── ManagementReviewController.cs ← calls _reviewsClient
│   │   ├── ImprovementController.cs      ← calls _improvementsClient
│   │   ├── ReportsController.cs          ← static view (no API calls yet)
│   │   ├── AdminController.cs            ← calls _usersClient, _orgsClient
│   │   ├── OrganizationsController.cs    ← calls _orgsClient, _usersClient, _standardsClient
│   │   ├── AccountController.cs          ← calls _authClient, _usersClient, _orgsClient
│   │   ├── LogsController.cs             ← calls _logsClient
│   │   └── WorkflowController.cs         ← static view (no API calls)
│   │
│   ├── Models/
│   │   └── ViewModels.cs
│   │
│   ├── Views/
│   │   ├── Home/ (Index.cshtml)
│   │   ├── Standards/ (Index.cshtml, Detail.cshtml)
│   │   ├── Controls/ (Index.cshtml, Detail.cshtml, Soa.cshtml)
│   │   ├── Risks/ (Index.cshtml, Detail.cshtml, Matrix.cshtml)
│   │   ├── Documents/ (Policies.cshtml, Processes.cshtml)
│   │   ├── Tasks/ (Index.cshtml, Kanban.cshtml, Calendar.cshtml)
│   │   ├── Evidence/ (Index.cshtml)
│   │   ├── Audits/ (Index.cshtml, Detail.cshtml)
│   │   ├── Findings/ (Index.cshtml, Detail.cshtml)
│   │   ├── Capa/ (Index.cshtml, Detail.cshtml)
│   │   ├── ManagementReview/ (Index.cshtml)
│   │   ├── Improvement/ (Index.cshtml)
│   │   ├── Reports/ (Index.cshtml)
│   │   ├── Admin/ (Users.cshtml)
│   │   ├── Organizations/ (Index.cshtml, Detail.cshtml)
│   │   ├── Account/ (Login.cshtml, ForgotPassword.cshtml, ResetPassword.cshtml, Profile.cshtml)
│   │   └── Shared/ (_Layout.cshtml, _TraceabilityBar.cshtml)
│   │
│   └── wwwroot/
│       ├── css/ (site.css)
│       └── js/
│           ├── site.js
│           ├── dashboard.js
│           ├── kanban.js
│           └── signalr-client.js
│
└── ISOFlow.Tests           (xUnit Test Project)
```

---

## API-First Data Flow Architecture

```text
┌─────────────────────────────────────────────────────────────────────────┐
│                         ISOFlow.Web (MVC UI Client)                     │
│  ┌──────────────┐    ┌─────────────────────┐    ┌───────────────────┐  │
│  │ Razor Views   │───▶│ MVC Controllers      │───▶│ Domain ApiClients │  │
│  │ (.cshtml)     │◀───│ (ViewData, ViewModel)│◀───│ (e.g. Standards)  │  │
│  └──────────────┘    └─────────────────────┘    └────────┬──────────┘  │
│                                                          │             │
│                                                 ┌────────▼──────────┐  │
│                                                 │ IApiHttpClient    │  │
│                                                 │ (Session JWT,     │  │
│                                                 │  ApiResponse<T>)  │  │
│                                                 └────────┬──────────┘  │
│  References: ISOFlow.Domain, ISOFlow.Application         │ HTTP        │
│  NO reference to ISOFlow.Infrastructure                  │ REST        │
└──────────────────────────────────────────────────────────┼─────────────┘
                                                           │
                                              http://localhost:5015/api/*
                                                           │
┌──────────────────────────────────────────────────────────┼─────────────┐
│                        ISOFlow.Api (REST API Server)     │             │
│  ┌──────────────────┐    ┌─────────────────┐    ┌───────▼──────────┐  │
│  │ ApiResponse<T>    │◀───│ API Controllers  │◀───│ HTTP Request     │  │
│  │ (JSON wrapper)    │───▶│ (Validation,     │───▶│ Pipeline         │  │
│  │                   │    │  Pagination,     │    │ (JWT, CORS,      │  │
│  │                   │    │  Error Handling) │    │  GlobalExcHandler)│  │
│  └──────────────────┘    └────────┬────────┘    └──────────────────┘  │
│                                   │                                    │
│                          ┌────────▼────────┐                           │
│                          │ Repository       │                           │
│                          │ Interfaces       │                           │
│                          │ (ISOFlow.        │                           │
│                          │  Application)    │                           │
│                          └────────┬────────┘                           │
│                                   │                                    │
│                          ┌────────▼────────┐                           │
│                          │ Concrete         │                           │
│                          │ Repositories     │                           │
│                          │ (ISOFlow.        │                           │
│                          │  Infrastructure) │                           │
│                          └─────────────────┘                           │
│  References: ISOFlow.Application, ISOFlow.Infrastructure               │
└────────────────────────────────────────────────────────────────────────┘
```

---

## Detailed MVC Controller → Modular API Client Mapping

Each MVC controller injects only its focused domain API client interface:

| MVC Controller | Injected Domain API Client | API Endpoints Consumed |
|---|---|---|
| `HomeController` | `IDashboardApiClient` | `GET /api/dashboard/kpis`, `trends`, `risk-matrix`, `traceability` |
| `StandardsController` | `IStandardsApiClient` | `GET/POST/PUT/DELETE /api/standards/*` |
| `ControlsController` | `IControlsApiClient` | `GET/POST/PUT/DELETE /api/controls/*` |
| `RisksController` | `IRisksApiClient` | `GET/POST/PUT/DELETE /api/risks/*` |
| `DocumentsController` | `IDocumentsApiClient` | `GET/POST/PUT/DELETE /api/documents/*` |
| `TasksController` | `ITasksApiClient` | `GET/POST/PUT/DELETE/PATCH /api/tasks/*` |
| `EvidenceController` | `IEvidenceApiClient` | `GET/POST/PUT/DELETE /api/evidence/*` |
| `AuditsController` | `IAuditsApiClient` | `GET/POST/PUT/DELETE /api/audits/*` |
| `FindingsController` | `IFindingsApiClient` | `GET/POST/PUT/DELETE /api/findings/*` |
| `CapaController` | `ICapaApiClient` | `GET/POST/PUT/DELETE/PATCH /api/capa/*` |
| `ManagementReviewController` | `IManagementReviewsApiClient` | `GET/POST/PUT/DELETE /api/managementreviews/*` |
| `ImprovementController` | `IImprovementsApiClient` | `GET/POST/PUT/DELETE /api/improvements/*` |
| `AdminController` | `IUsersApiClient`, `IOrganizationsApiClient` | User Management & Organizations via API |
| `OrganizationsController` | `IOrganizationsApiClient`, `IUsersApiClient`, `IStandardsApiClient` | Org Tenant CRUD & Directory via API |
| `AccountController` | `IAuthApiClient`, `IUsersApiClient`, `IOrganizationsApiClient` | Login (JWT), Profile, Password via API |
| `LogsController` | `ILogsApiClient` | `GET /api/logs` Log Filtering via API |

---

## ISOFlow.Web `Program.cs` — Service Registration

```csharp
// Register Base IApiHttpClient with BaseAddress and Session token attachment
builder.Services.AddHttpClient<IApiHttpClient, ApiHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register Domain-Specific API Clients
builder.Services.AddScoped<IDashboardApiClient, DashboardApiClient>();
builder.Services.AddScoped<IStandardsApiClient, StandardsApiClient>();
builder.Services.AddScoped<IControlsApiClient, ControlsApiClient>();
builder.Services.AddScoped<IRisksApiClient, RisksApiClient>();
builder.Services.AddScoped<IDocumentsApiClient, DocumentsApiClient>();
builder.Services.AddScoped<ITasksApiClient, TasksApiClient>();
builder.Services.AddScoped<IEvidenceApiClient, EvidenceApiClient>();
builder.Services.AddScoped<IAuditsApiClient, AuditsApiClient>();
builder.Services.AddScoped<IFindingsApiClient, FindingsApiClient>();
builder.Services.AddScoped<ICapaApiClient, CapaApiClient>();
builder.Services.AddScoped<IManagementReviewsApiClient, ManagementReviewsApiClient>();
builder.Services.AddScoped<IImprovementsApiClient, ImprovementsApiClient>();
builder.Services.AddScoped<IUsersApiClient, UsersApiClient>();
builder.Services.AddScoped<IOrganizationsApiClient, OrganizationsApiClient>();
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<ILogsApiClient, LogsApiClient>();
```

---

## Authentication Flow (Session-Stored JWT)

```text
1. User submits Login form → AccountController.Login(email, password)
2. AccountController calls _authClient.LoginAsync(email, password)
3. ApiHttpClient sends POST /api/auth/login to API
4. API validates credentials → returns LoginResponseDto (JWT + User Profile)
5. AccountController stores JWT token in Session: Session["ApiToken"] = jwt
6. AccountController stores user details in Session (Name, Role, OrgId, etc.)
7. All subsequent API calls via ApiHttpClient automatically attach: Authorization: Bearer {token}
```

---

## Verification Plan

### Automated Verification
1. Run `dotnet build ISOFlow.slnx` to confirm zero compilation errors.
2. Run `dotnet test ISOFlow.slnx` to execute unit tests.

### Manual & Visual Verification
1. Start `ISOFlow.Api` on `http://localhost:5015` — verify Swagger at `/swagger`.
2. Start `ISOFlow.Web` on `http://localhost:5285` — verify login and all pages load data from API.
3. Create/Edit/Delete a Standard, Control, Risk — confirm data persists via single API data store.
4. Verify TempData success/error messages display correctly after API operations.
5. Verify JWT auth: login → token stored → protected API calls succeed.
