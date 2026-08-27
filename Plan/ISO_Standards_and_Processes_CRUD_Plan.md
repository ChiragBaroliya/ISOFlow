# Implementation Plan — ISO Compliance Standards & Business Processes CRUD with Protection & Versioning

This plan details the addition of full **Create, Read, Update, Archive/Delete (CRUD)** capabilities for **ISO Compliance Standards** and **Business Processes** in **ISOFlow**, featuring role-based permission safeguards, pre-seeded protection rules, versioning, and interactive process flowcharts.

---

## Technical Architecture & Design Rules

> [!IMPORTANT]
> **ISO Standards Protection & Versioning Rules**:
> 1. **Official vs. Custom Standards**:
>    - Official pre-seeded standards (`ISO 27001:2022`, `ISO 9001:2015`, `ISO 14001:2015`) have `IsPreseeded = true`.
>    - Official standards protect core clause/requirement integrity and prevent accidental deletion. Editing allows scope/target compliance adjustments.
>    - Custom enterprise standards (`IsPreseeded = false`) support full create, edit, and deletion workflows.
> 
> 2. **Business Process Dynamic Lifecycle**:
>    - Full CRUD support: Create, Edit (with dynamic step ordering), and Archive for processes (`PROC-001` through `PROC-005` and new user-created processes).
>    - Integration with linked policies (`POL-001`, `POL-003`) and linked controls (`CTRL-001` through `CTRL-006`).
> 
> 3. **Role-Based Permissions**:
>    - `Compliance Manager` and `Admin` roles have full Create/Edit/Archive capabilities.
>    - `Auditor` and standard users receive read-only views with protected action triggers.

---

## User Review Required

> [!IMPORTANT]
> **Key Features to be Implemented**:
> - **Domain Model Enhancements**: Add `IsPreseeded` and `Status` flags to `Standard` entity; add `Status` and `Version` to `Process` entity.
> - **Repository Expansion**: Add `CreateStandardAsync`, `UpdateStandardAsync`, `DeleteStandardAsync` to `IStandardRepository` and `CreateProcessAsync`, `UpdateProcessAsync`, `ArchiveProcessAsync` to `IDocumentRepository`.
> - **Standards Management UI**:
>   - "Add Custom Standard" modal on `/Standards/Index`.
>   - "Edit Standard" modal supporting scope/compliance target updates for official standards, and full editing for custom standards.
>   - Protection badge and lock indicator on official standards (`ISO 27001:2022`, etc.).
> - **Business Processes Visualizer & Management UI**:
>   - Process tab bar on `/Documents/Processes` switching between `PROC-001` (JML), `PROC-002` (Incident Management), `PROC-003` (Supplier Assessment), `PROC-004` (Backup & DR), and `PROC-005` (CAPA).
>   - "Add Business Process" modal with step builder.
>   - "Edit Business Process" modal updating steps, owner, category, and policy link.
>   - Dynamic process step flowchart renderer.

---

## Proposed Changes

### Core Domain & Infrastructure Layer

#### [MODIFY] [ComplianceEntities.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Domain/Entities/ComplianceEntities.cs)
- Update `Standard` class to include:
  - `public bool IsPreseeded { get; set; } = false;`
  - `public string Status { get; set; } = "Active";`
- Update `Process` class to include:
  - `public string Status { get; set; } = "Active";`
  - `public string Version { get; set; } = "1.0";`

#### [MODIFY] [AcmeMockDataSeed.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Infrastructure/MockData/AcmeMockDataSeed.cs)
- Set `IsPreseeded = true` for `ISO-27001-2022`, `ISO-9001-2015`, and `ISO-14001-2015`.
- Seed 5 full ISO standard processes (`PROC-001` through `PROC-005`) with steps, owners, and control links.

#### [MODIFY] [IRepositories.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Application/Interfaces/IRepositories.cs)
- Add standard management methods to `IStandardRepository`:
  - `Task<Standard> CreateStandardAsync(Standard standard);`
  - `Task<Standard?> UpdateStandardAsync(Standard standard);`
  - `Task<bool> DeleteStandardAsync(string id);`
- Add process management methods to `IDocumentRepository`:
  - `Task<Process> CreateProcessAsync(Process process);`
  - `Task<Process?> UpdateProcessAsync(Process process);`
  - `Task<bool> ArchiveProcessAsync(string id);`

#### [MODIFY] [MockRepositories.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Infrastructure/Repositories/MockRepositories.cs)
- Implement `CreateStandardAsync`, `UpdateStandardAsync`, `DeleteStandardAsync` in `StandardRepository`.
- Implement `CreateProcessAsync`, `UpdateProcessAsync`, `ArchiveProcessAsync` in `DocumentRepository`.

---

### MVC Controllers & Web UI Layer

#### [MODIFY] [StandardsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/StandardsController.cs)
- Add `Create` (POST action) to handle standard creation.
- Add `Edit` (POST action) to handle standard updates (with protection check for pre-seeded official standards).
- Add `Delete` (POST action) to handle custom standard removal.

#### [MODIFY] [DocumentsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/DocumentsController.cs)
- Update `Processes(string? id)` (GET action) to load process catalog and selected process model.
- Add `CreateProcess` (POST action) to handle process creation.
- Add `EditProcess` (POST action) to handle process updates and step modifications.
- Add `ArchiveProcess` (POST action) to handle process archiving.

#### [MODIFY] [Index.cshtml](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Views/Standards/Index.cshtml)
- Add "Add Custom Standard" modal trigger button.
- Add "Edit Standard" modal per standard card.
- Add "Protected Official Standard" badge & lock indicator for pre-seeded standards.
- Add Create / Edit modal Razor partial markup.

#### [MODIFY] [Processes.cshtml](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Views/Documents/Processes.cshtml)
- Add interactive process selection tab bar (`PROC-001` to `PROC-005` + custom).
- Add "Add New Process" modal trigger button.
- Add "Edit Process" modal trigger button.
- Render dynamic flowchart cards with step numbers, descriptions, and linked control tags.

---

### Automated Unit Testing

#### [MODIFY] [ComplianceTraceabilityTests.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Tests/ComplianceTraceabilityTests.cs)
- Add unit tests verifying:
  - Standard creation, update, and pre-seeded protection checks.
  - Process creation, update, step retrieval, and archiving.

---

## Verification Plan

### Automated Tests
- Run `dotnet test ISOFlow.slnx` to verify domain model updates, repository CRUD logic, and protection rules.

### Manual & Visual Verification
1. Launch `ISOFlow.Web` using `dotnet run` on `http://localhost:5198`.
2. Navigate to **ISO Standards** (`/Standards`):
   - Verify official standards show "Protected Standard" lock badge.
   - Click "Add Custom Standard", enter custom standard details (e.g. `SOC 2 Type II`), and submit. Verify it appears in the catalog.
   - Test editing standard details.
3. Navigate to **Business Processes** (`/Documents/Processes`):
   - Switch between processes (`PROC-001` JML, `PROC-002` Incident Management, `PROC-003` Vendor Risk, `PROC-004` Backup DR, `PROC-005` CAPA) using the tab selector.
   - Verify dynamic flowchart rendering for each step.
   - Click "Add Process", create a new process with custom steps, and verify it renders immediately.
   - Click "Edit Process" and verify updating step details.
