# Implementation Plan — Organization-Wise CRUD & Tenant Login Architecture

Implement multi-tenant organization support in **ISOFlow**, including **Organization CRUD management** (List, Create, Edit, Tenant Overview), **Org-Wise Login & Tenant Switching**, and **Data Segregation** per Organization.

---

## Key Features & Requirements

> [!IMPORTANT]
> **1. Organization Entity & Multi-Tenant Data Scoping**:
> - **Domain Entity**: Expand `Organization` entity with `Code`, `Status` (`Active`, `Trial`, `Suspended`), `CreatedAt`, and `ContactEmail`.
> - **User & Data Link**: Link users and compliance data (`Controls`, `Risks`, `Policies`, `Processes`, `Tasks`, `Audits`) to an `OrganizationId`.
> - **Pre-seeded Tenants**:
>   1. `ORG-001`: **Acme Technologies Pvt. Ltd.** (Tech/SaaS • ISO 27001:2022)
>   2. `ORG-002`: **CyberShield Global Security** (Cybersecurity • ISO 27001:2022)
>   3. `ORG-003`: **Nexus Health Systems** (Healthcare • ISO 9001:2015)
> 
> **2. Org-Wise Login & Tenant Session Management**:
> - **Account Login View (`/Account/Login`)**: Interactive Organization Login screen supporting credentials entry or instant demo tenant login (Acme, CyberShield, Nexus).
> - **Active Tenant Context**: Persist `ActiveOrganizationId` and `ActiveUserId` in Session/Cookie context.
> - **TopBar Tenant Switcher**: Quick dropdown in top header allowing instant switching between organizations.
> 
> **3. Organization CRUD Management UI**:
> - **Directory View (`/Organizations`)**: Card layout showing all registered enterprise tenants, active standards, employee headcount, status, and compliance score.
> - **Create Modal**: Register new enterprise organization tenant.
> - **Edit Modal**: Update organization industry, headcount, locations, and primary ISO standard.
> - **Tenant Detail View (`/Organizations/Detail/{id}`)**: Single tenant profile dashboard showing organization compliance posture, assigned standards, and users.

---

## Proposed Technical Changes

### Core Domain & Application Layer

#### [MODIFY] [ComplianceEntities.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Domain/Entities/ComplianceEntities.cs)
- Update `Organization` class:
  - Add `public string Code { get; set; } = string.Empty;`
  - Add `public string Status { get; set; } = "Active";`
  - Add `public DateTime CreatedAt { get; set; } = DateTime.UtcNow;`
  - Add `public string ContactEmail { get; set; } = string.Empty;`
- Update `User` class:
  - Add `public string OrganizationId { get; set; } = "ORG-001";`

#### [MODIFY] [IRepositories.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Application/Interfaces/IRepositories.cs)
- Add `IOrganizationRepository` interface:
  - `Task<List<Organization>> GetAllOrganizationsAsync();`
  - `Task<Organization?> GetOrganizationByIdAsync(string id);`
  - `Task<Organization> CreateOrganizationAsync(Organization organization);`
  - `Task<Organization?> UpdateOrganizationAsync(Organization organization);`
  - `Task<bool> DeleteOrganizationAsync(string id);`

#### [MODIFY] [AcmeMockDataSeed.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Infrastructure/MockData/AcmeMockDataSeed.cs) & [MockRepositories.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Infrastructure/Repositories/MockRepositories.cs)
- Seed 3 distinct Organizations (`ORG-001`, `ORG-002`, `ORG-003`).
- Implement `OrganizationRepository` in `MockRepositories.cs`.

---

### MVC Controllers & Web UI Layer

#### [NEW] [AccountController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/AccountController.cs)
- `Login()` (GET & POST): Handles organization tenant authentication and sets Session cookies.
- `Logout()` (GET): Clears session context.
- `SwitchTenant(string orgId)` (POST): Switches active tenant context instantly.

#### [NEW] [OrganizationsController.cs](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Controllers/OrganizationsController.cs)
- `Index()`: List all organization tenants with status badges and metrics.
- `Detail(string id)`: View organization profile and compliance summary.
- `Create(Organization org)`: POST handler to register a new tenant.
- `Edit(Organization org)`: POST handler to update tenant metadata.
- `Delete(string id)`: POST handler to deactivate/delete custom tenant.

#### [NEW] Views for Account & Organizations:
- `Views/Account/Login.cshtml`: Modern enterprise login screen with Org selection tabs and demo tenant cards.
- `Views/Organizations/Index.cshtml`: Organization directory with Add/Edit modals.
- `Views/Organizations/Detail.cshtml`: Tenant overview dashboard.

#### [MODIFY] [Views/Shared/_Layout.cshtml](file:///d:/LearningProject/ISOFlow/ISOFlow.Web/Views/Shared/_Layout.cshtml)
- Add Active Organization indicator badge and Tenant Switcher dropdown in the topbar header.
- Add "Organizations" menu item under ORGANIZATION section in the sidebar.

---

## Verification Plan

### Automated Tests
- Add unit tests in `ISOFlow.Tests` verifying:
  - Organization CRUD operations (Create, Read, Update, Delete).
  - Multi-tenant tenant resolution and organization isolation.
- Run `dotnet test ISOFlow.slnx`.

### Manual & Visual Verification
1. Launch `ISOFlow.Web` via `dotnet run`.
2. Navigate to `/Account/Login`:
   - Test logging in as **Acme Technologies** (`ORG-001`).
   - Test logging in as **CyberShield Global** (`ORG-002`).
3. Verify topbar displays active Organization name and logo badge.
4. Test instant tenant switching via topbar switcher dropdown.
5. Navigate to `/Organizations`:
   - View directory of enterprise tenants.
   - Click "Add Organization" and create a new tenant (e.g. *Apex Logistics*).
   - Test editing tenant details.
