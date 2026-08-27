# Master Implementation Plan — ISOFlow Architecture & Lifecycle

Build a fresh, modern, enterprise-grade **ISO Compliance Management Platform** called **ISOFlow** targeting .NET 10, ASP.NET Core MVC, Web API, and clean software architecture.

ISOFlow orchestrates the complete ISO compliance lifecycle through **Compliance Traceability**:
Every entity in the platform is connected and navigable in both forward and backward directions.

---

## Technical Update: Modular ASP.NET Core MVC Architecture for ISOFlow.Web

> [!IMPORTANT]
> **Web UI Architecture Update**:
> - **Framework**: **ASP.NET Core MVC** (.NET 10).
> - **Modular Controller Files**: Every MVC controller resides in its own dedicated C# source file inside `ISOFlow.Web/Controllers/` (no single monolithic controller file).
> - **`ISOFlow.Web` Folder Structure**:
>   - **`Controllers/`**: 14 separate Controller files (`HomeController.cs`, `StandardsController.cs`, `ControlsController.cs`, `RisksController.cs`, `DocumentsController.cs`, `TasksController.cs`, `EvidenceController.cs`, `AuditsController.cs`, `FindingsController.cs`, `CapaController.cs`, `ManagementReviewController.cs`, `ImprovementController.cs`, `ReportsController.cs`, `AdminController.cs`).
>   - **`Models/`**: Dedicated ViewModels (`DashboardViewModel.cs`, `ControlDetailViewModel.cs`, `RiskDetailViewModel.cs`, `TaskKanbanViewModel.cs`).
>   - **`Views/`**: Razor `.cshtml` view templates grouped cleanly by Controller folder (`Home/`, `Standards/`, `Controls/`, `Risks/`, `Documents/`, `Tasks/`, `Evidence/`, `Audits/`, `Findings/`, `Capa/`, `ManagementReview/`, `Improvement/`, `Reports/`, `Admin/`, `Shared/`).
>   - **`wwwroot/js/`**: Modularized client JavaScript files (`site.js`, `dashboard.js`, `kanban.js`, `signalr-client.js`).
> - **Removal of Legacy Pages**: The legacy Razor Pages directory (`Pages/`) is completely removed.

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

When a user opens **CTRL-001 (User Access Management)**, `ControlsController` serves the 12-tab control view containing:
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

## Solution & MVC Project Structure

```text
ISOFlow.sln
│
├── ISOFlow.Domain          (Class Library - Domain Entities, Enums, Value Objects)
├── ISOFlow.Application     (Class Library - DTOs, Repository Interfaces, Application Services)
├── ISOFlow.Infrastructure  (Class Library - Relational Mock Repository Store, IMemoryCache Service)
├── ISOFlow.Api             (ASP.NET Core Web API - REST Controllers, ComplianceHub SignalR, Swagger)
│
├── ISOFlow.Web             (ASP.NET Core MVC Project)
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── StandardsController.cs
│   │   ├── ControlsController.cs
│   │   ├── RisksController.cs
│   │   ├── DocumentsController.cs
│   │   ├── TasksController.cs
│   │   ├── EvidenceController.cs
│   │   ├── AuditsController.cs
│   │   ├── FindingsController.cs
│   │   ├── CapaController.cs
│   │   ├── ManagementReviewController.cs
│   │   ├── ImprovementController.cs
│   │   ├── ReportsController.cs
│   │   └── AdminController.cs
│   │
│   ├── Models/
│   │   └── ViewModels.cs (or individual ViewModels)
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

## Detailed MVC Component Plan

#### 1. Individual Controller Files (`ISOFlow.Web/Controllers/`)
- `HomeController.cs`: Dashboard routing and KPI fetching.
- `StandardsController.cs`: Standards list and requirement clause routing.
- `ControlsController.cs`: Controls register, 12-tab `CTRL-001` view, and SoA grid routing.
- `RisksController.cs`: Risk register, 5x5 heatmap, and treatment routing.
- `DocumentsController.cs`: Policies repository and JML process diagram routing.
- `TasksController.cs`: Tasks list, Kanban board, and calendar routing.
- `EvidenceController.cs`: Objective evidence vault routing.
- `AuditsController.cs`: Audit programs and `AUD-2026-001` audit detail routing.
- `FindingsController.cs`: `FIND-001` finding detail routing.
- `CapaController.cs`: `CAPA-001` corrective action routing.
- `ManagementReviewController.cs`: Q4 executive review routing.
- `ImprovementController.cs`: `IMP-001` continual improvement routing.
- `ReportsController.cs`: Compliance reports hub routing.
- `AdminController.cs`: RBAC roles and users routing.

---

## Verification Plan

### Automated Verification
1. Run `dotnet build` to confirm zero compilation errors across the 14 separate controller files.
2. Run `dotnet test` to execute unit tests verifying domain entity relationships and traceability graph traversal.

### Manual & Visual Verification
1. Launch `ISOFlow.Web` (MVC) using `dotnet run` on `http://localhost:5198`.
2. Verify MVC controller action execution and view rendering for all routes.
