# ISOFlow — Pending Implementation Checklist

_Last reviewed: 2026-10-26 (against `master` @ `3158cd0`)_

**Rule: no mock or hardcoded data.** Every value shown in the UI must come from PostgreSQL through
`ISOFlow.Api`. The only place sample records may exist is the opt-in seed script
`ISOFlow.Domain/DatabaseSchema/04_Seed_Data.sql`.

---

## 0. Blockers (do first)

- [x] **Fix the broken build.** `dotnet build ISOFlow.slnx` failed with 4 errors: `ISOFlow.Web.Services.Logs` / `ILogsApiClient` not found.
  - Cause: the `[Ll]ogs/` rule in `.gitignore` silently excluded `ISOFlow.Web/Services/Logs/` and `ISOFlow.Web/Views/Logs/` from git, so neither was ever committed.
  - [x] Added negation rules to `.gitignore` (`!ISOFlow.Web/Services/Logs/`, `!ISOFlow.Web/Views/Logs/`)
  - [x] Recreated `Services/Logs/LogsApiClient.cs` (`ILogsApiClient` + `LogsApiClient`, calls `GET /api/logs`)
  - [x] Recreated `Views/Logs/Index.cshtml` (date range, level pills, search, expandable stack traces)
  - [x] Renamed the API class `LogsApiController` → `LogsController`, so its route is `/api/logs` like the other controllers (it was `/api/LogsApi`). Updated `JwtAuthenticationTests`.
  - [x] `dotnet build ISOFlow.slnx` passes: 0 errors, 0 warnings
- [x] Ran `dotnet test ISOFlow.slnx`: **73 passed, 7 failed**. All 7 failures are `28P01: password authentication failed for user "postgres"`, because the tests hardcode `Password=postgres` (`UnitTest1.cs:23`, `ComplianceTraceabilityTests.cs:14`).
  - [ ] Move the test connection string to config or an environment variable (see §1.3), then re-run against a seeded PostgreSQL database

---

## 1. Remove all mock data (mandatory)

The `MockStore.*` in-memory store has already been deleted from Infrastructure. Mock and demo data
is still hardcoded in the places below.

### 1.1 Domain / Application / API

- [x] `User.cs` — `Password = "Test@123"` replaced by `PasswordHash` (no default, `[JsonIgnore]`)
- [x] `User.cs` — mock reset token replaced by `ResetTokenHash` (SHA-256) and `ResetTokenExpiresAt` (30 min), both `[JsonIgnore]`
- [x] `UserRequestDto.Password` — no default. It's required on create (API returns 400 without it) and optional on update (blank keeps the current password).
- [x] `PasswordSecurityHelper` — plain-text fallback removed. Added `IsPasswordHash` and `HashResetToken`, and reset tokens are now 32 chars and unbiased. Seed passwords in `04_Seed_Data.sql` are PBKDF2 hashes.
- [x] `IUserRepository` — contract and docs updated (hash-only storage, expiring hashed reset tokens)
- [x] `DashboardService.GetComplianceTrendsAsync` — fabricated months removed. It now records the live score monthly in the new `compliance_snapshots` table and returns the stored history (up to 12 months). History starts empty and grows each month.
- [x] `GetGoldenScenarioTraceabilityAsync("CTRL-001")` renamed to `GetControlTraceabilityAsync(controlId, …)`. `GET /api/dashboard/traceability?controlId=…` is now required (400 if missing).
- [x] Swagger contact moved to `Swagger:Contact:Name/Email` config (empty by default, so no contact is shown)
- **Security fixes found while doing this:**
  - [x] `CreateUserAsync`, `ResetPasswordAsync` and `ChangePasswordAsync` stored **plain-text** passwords. All writes now hash with PBKDF2, and change-password verifies against the hash.
  - [x] API user endpoints returned the full `User` entity, **including the password and reset token**. They're now hidden via `[JsonIgnore]`.
  - [x] Admin-created users silently got `Test@123` (the form had no password field). The Admin form now has *Initial Password* (create) and *New Password* (edit) fields.
  - [x] Setting a password through `PUT /api/users/{id}` was silently ignored. `UpdateUserAsync` now persists it.
  - [x] Removed `sp_user_validate_login` (it compared plain-text passwords)
- **DB action required for existing databases:** run `ISOFlow.Domain/DatabaseSchema/05_Migration_Security_And_Snapshots.sql` once. It converts `Test@123` rows to the hash, adds `reset_token_expires_at`, invalidates old reset tokens, and creates `compliance_snapshots`. Any user with another plain-text password must use *Forgot password*; the script lists them.

### 1.2 Web — Views with hardcoded entities and numbers

- [ ] `Views/Home/Index.cshtml` — static KPI values (`82.5%`, `5`, `12`, `3`, `88%`) and text (`FIND-001 Major NC active`, `CAPA-001 In Progress`, line 155 `controlId = "CTRL-001"`). Bind them to `DashboardViewModel.Kpis`.
- [ ] `wwwroot/js/dashboard.js:11-14` — hardcoded chart labels (`Jan…Jul`) and data (`72.0 … 82.5`). Read them from `Model.Trends`.
- [ ] `Views/Shared/_Layout.cshtml:229-233` — static notifications dropdown ("5 compliance tasks overdue", "Major NC FIND-001", "Access Control Policy due…"). Wire it to `NotificationsApiController` (see §2).
- [ ] `Views/Shared/_TraceabilityBar.cshtml` — every node is a hardcoded `CTRL-001 / RISK-001 / TRT-001 / AUD-2026-001 / FIND-001 / CAPA-001` link. Build the chain dynamically from `GET /api/traceability/{id}`.
- [ ] `Views/Shared/Components/RelatedComplianceItems/Default.cshtml:21` — hardcoded `RISK-001` link
- [ ] `Components/RelatedComplianceItemsViewComponent.cs` — default `controlId = "CTRL-001"`
- [ ] `Views/Controls/Detail.cshtml` — risks, findings, CAPA, and audit sections (lines ~75-245) are static `RISK-001/002`, `FIND-001`, `CAPA-001`, `AUD-2026-001`. Bind them to the related-items API.
- [ ] `Views/Risks/Detail.cshtml:38-41` — hardcoded `CTRL-001` link
- [ ] `Views/Audits/Detail.cshtml` — fully static (checklist `CTRL-001/002/006`, `FIND-001`, fallback `"AUD-2026-001"`). Bind it to the audit model.
- [ ] `Views/Findings/Detail.cshtml` — fully static (control, audit, `CAPA-001`, fallback `"FIND-001"`)
- [ ] `Views/Capa/Detail.cshtml` — fallbacks `"CAPA-001"` (lines 3, 17, 84)
- [ ] `Views/Findings/Index.cshtml:173` — create form pre-filled with `value="AUD-2026-001"`
- [ ] Index views that highlight a fixed demo row: `Audits/Index:90`, `Capa/Index:81`, `Controls/Index:92`, `Findings/Index:92`, `Risks/Index:93`. Remove the hardcoded ids.
- [ ] `Views/Risks/Matrix.cshtml` — static matrix. Bind it to `GET /api/dashboard/risk-matrix`.
- [ ] `Views/Tasks/Calendar.cshtml` — static placeholder. Build a real calendar from the tasks API.
- [ ] `Views/Reports/Index.cshtml` — `alert('Downloading…')` stubs and the hardcoded `AUD-2026-001` text (see §3)
- [x] `Views/Admin/Users.cshtml:190` — "All users share the mock password `Test@123`" removed (done with §1.1)
- [ ] `Views/Account/Profile.cshtml:225` — "Mock mode: current password is `Test@123`…". Remove it.
- [ ] `Views/Account/ForgotPassword.cshtml:5,133-141` and `Controllers/AccountController.cs:186` — the reset token is shown on screen as "Mock Token (Dev Mode)". Send it by email instead and stop returning it from the API.
- [ ] Controllers that set `ViewData["ActiveTraceabilityId"] = "CTRL-001"` (`HomeController`, `ReportsController`, …). Derive it from the current entity.
- [ ] `Views/Workflow/Index.cshtml` (959 lines, static). Decide whether to keep it as documentation or remove it.

### 1.3 Tests

- [ ] `ISOFlow.Tests/JwtAuthenticationTests.cs:41` and `UserAuthorizationTests.cs:41` — `Database=isoflow_mock` connection string. Move it to test configuration or use Testcontainers PostgreSQL.
- [ ] In-memory test doubles (`MockUserRepository`, `FakeAuditLogRepository`) are fine for unit tests. Add **integration tests** against a real PostgreSQL database for every repository.

### 1.4 Documentation

- [ ] `implementation_plan.md:143` and `Plan/Master_ISOFlow_Architecture_Plan.md:118` still describe Infrastructure as a "Relational Mock Repository Store". Update them to "PostgreSQL + Dapper repositories".
- [ ] `Plan/ISO_Standards_and_Processes_CRUD_Plan.md` and `Plan/Organization_MultiTenancy_Plan.md` reference the deleted `AcmeMockDataSeed.cs` / `MockRepositories.cs`. Mark them as superseded or update them.

---

## 2. Missing features / wiring

- [ ] **Notifications**: `NotificationsApiController` exists, but Web has no `NotificationsApiClient`.
  - [ ] Add `Services/Notifications/INotificationsApiClient.cs` + `NotificationsApiClient.cs` and register them in `ISOFlow.Web/Program.cs`
  - [ ] Add a notifications ViewComponent for the layout bell (unread count + list + mark-as-read)
- [ ] **SignalR real-time updates**: `ComplianceHub` defines `ReceiveNotification`, `KpiUpdated`, and `TaskUpdated`, but no API controller injects `IHubContext<ComplianceHub>`, so nothing is ever broadcast.
  - [ ] Broadcast on task status changes, CAPA/finding changes, and KPI recalculation
  - [ ] Scope broadcasts per organization (SignalR groups), not `Clients.All`
  - [ ] Verify `wwwroot/js/signalr-client.js` connects using the JWT
- [ ] **Traceability API client**: Web has no `TraceabilityApiClient`, which is needed for the dynamic traceability bar (§1.2)
- [ ] **Dashboard**: the `GET /api/dashboard/traceability` endpoint isn't consumed by Web
- [ ] **Evidence file upload**: the form takes a free-text `FileUrl`. Implement real upload/download storage (local disk or blob) with size and type validation.
- [ ] **Policies file path**: same as Evidence. `FilePath` is free text.
- [ ] **Forgot / reset password**: email delivery service (SMTP/SendGrid), hashed single-use token with expiry
- [ ] **Recurring tasks**: `TaskTemplate` + `TaskFrequencyExtensions` exist. Add a background job (`IHostedService`) that generates tasks from templates.
- [ ] **Overdue detection**: background job that flags overdue tasks and evidence expiry, then raises notifications
- [ ] Free-text cross-reference inputs (`ControlId`, `RequirementId`, `AuditId`, `FindingId`, `TaskId`, `Owner`…). Replace them with dropdowns or autocomplete fed by the API.

---

## 3. Reports module

- [ ] `ReportsController` has no API calls, and the view only shows `alert()` stubs
- [ ] ISO 27001 executive summary — PDF export
- [ ] Risk register — Excel export
- [ ] Audit & CAPA report — PDF export
- [ ] Statement of Applicability (SoA) — export
- [ ] API endpoints for report generation (`/api/reports/*`)

---

## 4. Security & configuration

- [ ] Move secrets out of `ISOFlow.Api/appsettings*.json` (JWT `Secret`, DB `Password=postgres`) into User Secrets / environment variables
- [ ] Remove the unused `Microsoft.Data.SqlClient` package from `ISOFlow.Infrastructure.csproj`, or document why it's needed (the DB is PostgreSQL/Npgsql)
- [ ] Review CORS policy for production
- [ ] Add rate limiting on `/api/auth/login` and `/api/auth/forgot-password`
- [ ] Review repository lifetimes: every repository and service is registered as `Singleton` in `ISOFlow.Api/Program.cs`. Confirm this is safe with `AmbientDbContext`, or switch to `Scoped`.

---

## 5. Database

- [ ] Add a migration strategy (versioned scripts with DbUp/Flyway, or EF migrations) instead of manual `01_…04_*.sql` execution
- [ ] Make `04_Seed_Data.sql` optional (dev only), and keep it out of production deployment
- [x] Add a `compliance_snapshots` table for historical trend data (done with §1.1)

---

## 6. Testing & verification

- [ ] Fix the build (§0), then make `dotnet test` green
- [ ] Integration tests for all repositories against PostgreSQL
- [ ] API controller tests for every module (only Users/Auth/AuditLogs are covered today)
- [ ] Manual check: every page shows **only** database data after the seed script is truncated (an empty DB should render empty states, not demo values)
- [ ] Manual check: create/edit/delete for each module persists via the API

---

## 7. DevOps

- [ ] CI pipeline (build + test on every PR)
- [ ] `README.md` with setup steps (PostgreSQL, run scripts, start API on `:5015` and Web on `:5285`)
- [ ] Dockerfile / docker-compose (API + Web + PostgreSQL)
