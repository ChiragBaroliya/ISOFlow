# Walkthrough — ISOFlow: Modular API-First Integration & UI Rewiring

We have transformed **ISOFlow.Web** into a **pure MVC UI client** that communicates exclusively with **ISOFlow.Api** over HTTP. All direct repository and infrastructure dependencies have been removed from the Web project, and all API calls have been cleanly modularized into separated, domain-focused API client services.

---

## 1. Key Accomplishments

### 🏗️ 1. API-First Architecture & Infrastructure Decoupling
- **Zero Infrastructure Reference**: Removed `<ProjectReference Include="..\ISOFlow.Infrastructure\ISOFlow.Infrastructure.csproj" />` from [ISOFlow.Web.csproj](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/ISOFlow.Web.csproj).
- **Single Source of Truth**: All data operations (queries, updates, mutations, authentication, logs) are executed by [ISOFlow.Api](file:///d:/LearningProject/ISOFlow/ISOFlow.Api) endpoints.
- **Data Flow**:
  $$\text{User / Browser} \longrightarrow \text{Razor View} \longrightarrow \text{MVC Controller} \longrightarrow \text{Domain API Client} \longrightarrow \text{IApiHttpClient} \longrightarrow \text{ISOFlow.Api REST Endpoints}$$

---

### 📦 2. Modular Domain-Separated API Clients (`ISOFlow.Web/Services/`)

Instead of a monolithic single API client file, each functional area has its own dedicated, typed API client interface and implementation:

| Domain Area | Interface & Implementation | Endpoints & Operations |
|---|---|---|
| **Base Core** | [IApiHttpClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Base/IApiHttpClient.cs), [ApiHttpClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Base/ApiHttpClient.cs) | Centralized HTTP verbs (`GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`, `PatchBoolAsync`), auto-attaches Session JWT Bearer token, unwraps `ApiResponse<T>`. |
| **Dashboard** | [DashboardApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Dashboard/DashboardApiClient.cs) | `GET /api/dashboard/kpis`, `trends`, `risk-matrix`, `traceability` |
| **Standards** | [StandardsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Standards/StandardsApiClient.cs) | `GET/POST/PUT/DELETE /api/standards/*` (Standards + Requirements) |
| **Controls** | [ControlsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Controls/ControlsApiClient.cs) | `GET/POST/PUT/DELETE /api/controls/*`, Statement of Applicability (SoA), Related Items |
| **Risks** | [RisksApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Risks/RisksApiClient.cs) | `GET/POST/PUT/DELETE /api/risks/*`, Risk Treatments, 5x5 Risk Heatmap |
| **Documents** | [DocumentsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Documents/DocumentsApiClient.cs) | `GET/POST/PUT/DELETE /api/documents/policies/*`, `processes/*`, Process Archival |
| **Tasks** | [TasksApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Tasks/TasksApiClient.cs) | `GET/POST/PUT/DELETE /api/tasks/*`, `PATCH /api/tasks/{id}/status` |
| **Evidence** | [EvidenceApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Evidence/EvidenceApiClient.cs) | `GET/POST/PUT/DELETE /api/evidence/*` (Audit evidence vault) |
| **Audits** | [AuditsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Audits/AuditsApiClient.cs) | `GET/POST/PUT/DELETE /api/audits/*` (Audit programs and checklists) |
| **Findings** | [FindingsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Findings/FindingsApiClient.cs) | `GET/POST/PUT/DELETE /api/findings/*` (Audit non-conformities) |
| **CAPA** | [CapaApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Capa/CapaApiClient.cs) | `GET/POST/PUT/DELETE /api/capa/*`, Action Item add & toggle |
| **Reviews** | [ManagementReviewsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/ManagementReviews/ManagementReviewsApiClient.cs) | `GET/POST/PUT/DELETE /api/managementreviews/*` |
| **Improvements**| [ImprovementsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Improvements/ImprovementsApiClient.cs) | `GET/POST/PUT/DELETE /api/improvements/*` (Continual improvement initiatives) |
| **Users** | [UsersApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Users/UsersApiClient.cs) | `GET/POST/PUT/DELETE /api/users/*`, Profile editing, Password change, Org users |
| **Organizations**| [OrganizationsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Organizations/OrganizationsApiClient.cs) | `GET/POST/PUT/DELETE /api/organizations/*` (Multi-tenant directory & CRUD) |
| **Auth** | [AuthApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Auth/AuthApiClient.cs) | `POST /api/auth/login`, `POST /api/auth/logout`, `forgot-password`, `reset-password` |
| **Logs** | [LogsApiClient.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Services/Logs/LogsApiClient.cs) | `GET /api/logs` with Date Range, Level, and Keyword filtering |

---

### 🎮 3. Complete Rewire of All 16 MVC Controllers

Every controller in [ISOFlow.Web/Controllers/](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/) has been rewired to inject only its dedicated domain client(s):

- [HomeController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/HomeController.cs): Injects `IDashboardApiClient` for KPIs, trends, and risk matrix.
- [StandardsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/StandardsController.cs): Injects `IStandardsApiClient` for standards and clause requirements.
- [ControlsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/ControlsController.cs): Injects `IControlsApiClient` for 12-tab control details, SoA, and related counts.
- [RisksController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/RisksController.cs): Injects `IRisksApiClient` for risk register, treatments, and heatmap.
- [DocumentsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/DocumentsController.cs): Injects `IDocumentsApiClient` for policies and process diagrams.
- [TasksController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/TasksController.cs): Injects `ITasksApiClient` for list, Kanban board, and status transitions.
- [EvidenceController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/EvidenceController.cs): Injects `IEvidenceApiClient` for evidence vault.
- [AuditsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/AuditsController.cs): Injects `IAuditsApiClient` for audit programs and scheduling.
- [FindingsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/FindingsController.cs): Injects `IFindingsApiClient` for non-conformity tracking.
- [CapaController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/CapaController.cs): Injects `ICapaApiClient` for CAPA workflows and action item toggling.
- [ManagementReviewController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/ManagementReviewController.cs): Injects `IManagementReviewsApiClient` for governance reviews.
- [ImprovementController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/ImprovementController.cs): Injects `IImprovementsApiClient` for continual improvement.
- [AdminController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/AdminController.cs): Injects `IUsersApiClient` and `IOrganizationsApiClient` for user administration.
- [OrganizationsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/OrganizationsController.cs): Injects `IOrganizationsApiClient`, `IUsersApiClient`, and `IStandardsApiClient` for tenant management.
- [AccountController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/AccountController.cs): Injects `IAuthApiClient`, `IUsersApiClient`, and `IOrganizationsApiClient` for login JWT acquisition, profile updates, password reset, and tenant switching.
- [LogsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/LogsController.cs): Injects `ILogsApiClient` for system logs.

---

### ⚙️ 4. Clean Service Configuration (`Program.cs` & `appsettings.json`)
- [ISOFlow.Web/appsettings.json](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/appsettings.json) now points directly to the API server:
  ```json
  "ApiSettings": {
    "BaseUrl": "http://localhost:5015"
  }
  ```
- [ISOFlow.Web/Program.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Program.cs) has **zero repository registrations**. All services are registered with Scoped lifetimes consuming `IApiHttpClient`.

---

## 2. Verification Results

### Automated Build
Executed `dotnet build ISOFlow.slnx`:
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:32.79
```

### Automated Unit Tests
Executed `dotnet test ISOFlow.slnx --no-build`:
```text
Passed!  - Failed: 0, Passed: 30, Skipped: 0, Total: 30, Duration: 4 s - ISOFlow.Tests.dll (net10.0)
```
All 30 unit tests across domain models, traceability graphs, and log parsing pass with 100% success.
