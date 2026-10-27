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
  - [x] Moved the test connection string to the `ISOFLOW_TEST_DB` environment variable (§1.3); all tests pass against a seeded PostgreSQL database

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

- [x] Home dashboard — all KPI cards bound to live KPIs. "vs last month" delta comes from real snapshots, the org name from the session, and the made-up "Audit Readiness 88%" card is removed. The golden-scenario component and button are removed.
- [x] `dashboard.js` — chart reads the server-rendered monthly snapshots (empty state when there's no history)
- [x] Layout — fake notifications replaced by an empty state; the hardcoded sidebar badges ("5", "3") and the dead `/#notifications` link are removed. *Live notifications are still pending, see §2.*
- [x] Traceability bar — the static partial is replaced by a `TraceabilityBar` view component that calls `GET /api/traceability/{code}` via a new `TraceabilityApiClient`. It renders only on detail pages with a linked chain.
- [x] **API `TraceabilityRepository` was itself mock data:** it ignored the requested entity and always returned the CTRL-001 chain, with invented fallback nodes. Rewritten to resolve the root (control/risk/task/evidence/finding/CAPA/audit) to its control and follow real FK and JSON links; missing steps are omitted.
- [x] `RelatedComplianceItems` component removed (hardcoded `RISK-001`, default `CTRL-001`, and it relied on a buggy count procedure); Control detail now computes counts from real lists
- [x] Control detail — every tab (requirement, risks, treatments, policies, processes, tasks, evidence, audits, findings, CAPA) built from real related records, with empty states
- [x] Risk detail — score, level, mapped control, treatment code and residual scores were hardcoded; now all live
- [x] Audit detail — checklist resolved from the audit's control list, with evaluation derived from the audit's findings and the evidence linked to it
- [x] Finding detail — requirement, control, audit and CAPAs resolved from the finding's links
- [x] CAPA detail — demo fallbacks and the "Q4 Management Review" / "IMP-001" buttons removed
- [x] Detail actions (Controls, Risks, Audits, Findings, CAPA, Standards) no longer silently show the *first* record for a missing or unknown id; they redirect to the list or return 404
- [x] Findings create form no longer pre-filled with `AUD-2026-001`
- [x] Demo-row highlights removed (Audits, CAPA, Controls, Findings, Risks, Policies, Standards requirements)
- [x] Risk Matrix — legend counts computed from the matrix data (grid was already live)
- [x] Tasks Calendar — real 6-month calendar from task due dates, audit start dates and CAPA due dates, plus an overdue list
- [x] Reports — `alert()` stubs replaced with links to the live pages plus disabled "export coming soon" buttons. *Real exports are pending, see §3.*
- [x] Profile "Mock mode: password is Test@123" note removed
- [x] **Forgot password (security fix):** the anonymous API returned the reset token to any caller (anyone could reset any password) and returned 404 for unknown emails (account enumeration). The token is now returned only in the Development environment, and the response is identical for all emails. *Email delivery is still pending, see §2.*
- [x] Hardcoded `ActiveTraceabilityId` demo values removed from all controllers; only detail pages set it, from the real entity
- [x] Login page "Quick Fill Demo Accounts" buttons (with `Test@123`) removed
- [x] Global search — replaced the hardcoded redirect to the demo records with code-prefix routing (`CTRL-`, `RISK-`, `FIND-`, `CAPA-`, `AUD-`, `ISO-`)
- [x] SuperAdmin tenant switcher — hardcoded Acme/CyberShield/Nexus entries (with ids `ORG-001`… that didn't match real ids) replaced by a component listing real organizations
- [x] Workflow page — **kept** as the product guide; its codes are labelled examples, not live data
- [ ] Follow-up: `sp_controls_get_related_items_count` matches JSON code lists against the numeric id with `ILIKE '%1%'` (false matches). Fix or remove it, since the Web no longer uses it.

### 1.3 Tests

- [x] Hardcoded test connection strings removed. Database tests read `ISOFLOW_TEST_DB` and are **skipped** (not failed) when it's unset, via `[DatabaseFact]` in `ISOFlow.Tests/Infrastructure/TestDatabase.cs`.
  - The `DatabaseFixture` rebuilds the test database from scripts 01–04 once per run. It refuses any database whose name doesn't contain `test`.
  - All database tests share one serial xUnit collection.
  - Local setup: create `isoflow_test`, then set `ISOFLOW_TEST_DB=Host=localhost;Port=5432;Database=isoflow_test;Username=postgres;Password=…`
- [x] Integration tests for every repository (`RepositoryIntegrationTests.cs`, 15 tests): users (hashing, login, change and reset with expiry), refresh tokens, organizations, audits/findings (tenant isolation), CAPA plus action items, evidence, policies/processes, reviews/improvements, notifications, audit log, compliance snapshots, and requirement/control link lists
- [x] Results: **102/102 pass** against PostgreSQL 18.3; without a database, 78 pass and 24 are skipped
- **Bugs found and fixed by these tests:**
  - [x] **`RefreshTokenRepository` was a mock in production.** It kept a static in-memory store and swallowed every database error, so failed logouts and revocations reported success. It's now database-only and errors propagate; the unit tests use `InMemoryRefreshTokenRepository`.
  - [x] **JSON list columns were never saved or loaded.** Affected: policy linked controls and processes, process steps and controls, audit checklist and findings, requirement links, all 12 control link lists, review attendees, and organization locations. Every list entered in the UI was silently dropped. New `BaseRepository.SaveJsonListsAsync` / `LoadJsonListsAsync` helpers now cover all 13 columns.
  - [x] **CAPA action items were saved but never loaded** (`GetCapaByIdAsync` now loads them)

### 1.4 Documentation

- [x] `implementation_plan.md` and `Plan/Master_ISOFlow_Architecture_Plan.md` now describe Infrastructure as "PostgreSQL + Dapper Repositories"
- [x] `Plan/ISO_Standards_and_Processes_CRUD_Plan.md` and `Plan/Organization_MultiTenancy_Plan.md` are marked as historical. Each has a banner saying the mock store was removed and pointing to the current repositories, schema and tests.
- [x] `DatabaseSchemaReadme.md` rewritten. It described SQL Server/T-SQL (`EXEC [dbo]…`, `:r`) and claimed "zero inline queries". It now has PostgreSQL `psql` setup, the 05 upgrade, test-database setup, dev seed accounts, tenant/JSON-link/credential rules, and examples verified against the database.

**Section 1 (mock data removal) is complete.**

---

## 2. Missing features / wiring

- [ ] **Notifications**: `NotificationsApiController` exists, but Web has no `NotificationsApiClient`.
  - [ ] Add `Services/Notifications/INotificationsApiClient.cs` + `NotificationsApiClient.cs` and register them in `ISOFlow.Web/Program.cs`
  - [ ] Add a notifications ViewComponent for the layout bell (unread count + list + mark-as-read)
- [ ] **SignalR real-time updates**: `ComplianceHub` defines `ReceiveNotification`, `KpiUpdated`, and `TaskUpdated`, but no API controller injects `IHubContext<ComplianceHub>`, so nothing is ever broadcast.
  - [ ] Broadcast on task status changes, CAPA/finding changes, and KPI recalculation
  - [ ] Scope broadcasts per organization (SignalR groups), not `Clients.All`
  - [ ] Verify `wwwroot/js/signalr-client.js` connects using the JWT
- [x] **Traceability API client**: added `TraceabilityApiClient` (done with §1.2)
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

- [x] Secrets removed from `appsettings*.json` (API and Web), and the hardcoded fallbacks removed from `JwtSettings` and `DbConnectionFactory`. The API now **refuses to start** without `JwtSettings:Secret` (32+ chars) and `ConnectionStrings:DefaultConnection`.
  - Development: User Secrets (`UserSecretsId` added to `ISOFlow.Api.csproj`)
  - Elsewhere: environment variables `JwtSettings__Secret` and `ConnectionStrings__DefaultConnection`
  - Local setup:
    ```bash
    cd ISOFlow.Api
    dotnet user-secrets set "JwtSettings:Secret" "<64 random chars>"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=isoflow;Username=postgres;Password=<password>"
    ```
- [x] **JWT secret rotated for development.** The old secrets are still in git history (treat them as public). A new random 64-char secret is in this machine's User Secrets. **Any other environment must also be given a fresh secret.**
- [x] Removed the unused `Microsoft.Data.SqlClient` package and the SQL Server code path in `DbConnectionFactory`
- [x] CORS — was "any origin with credentials". Now only `Cors:AllowedOrigins`: empty by default; Development lists the Web app's local origins (needed later for SignalR). Verified: a foreign origin gets no `Access-Control-Allow-Origin`.
- [x] Swagger — was on in every environment. Now only when `Swagger:Enabled` is true (set in `appsettings.Development.json`); the root redirect follows the same setting. Verified off in Production.
- [x] Rate limiting — built-in limiter, fixed window per client IP (`RateLimiting:Auth`, default 10 requests per 60 s), shared across `auth/login`, `auth/refresh`, `users/login`, `users/forgot-password` and `users/reset-password`. Returns 429 with `Retry-After` and an `ApiResponse` body. Verified on the running API.
  - [ ] Follow-up: behind a reverse proxy, enable forwarded headers so the limiter sees the real client IP
- [x] Repository lifetimes reviewed: singletons are **safe**. They hold no per-request state, each call opens and disposes its own connection, and `AmbientDbContext` uses `AsyncLocal` (per request flow). Documented in `Program.cs`.

---

## 5. Database

- [ ] Add a migration strategy (versioned scripts with DbUp/Flyway, or EF migrations) instead of manual `01_…04_*.sql` execution
- [ ] Make `04_Seed_Data.sql` optional (dev only), and keep it out of production deployment
- [x] Add a `compliance_snapshots` table for historical trend data (done with §1.1)

---

## 6. Testing & verification

- [x] Fix the build (§0), then make `dotnet test` green
- [x] Integration tests for all repositories against PostgreSQL (§1.3)
- [ ] API controller tests for every module (only Users/Auth/AuditLogs are covered today)
- [ ] Manual check: every page shows **only** database data after the seed script is truncated (an empty DB should render empty states, not demo values)
- [ ] Manual check: create/edit/delete for each module persists via the API

---

## 7. DevOps

- [ ] CI pipeline (build + test on every PR)
- [ ] `README.md` with setup steps (PostgreSQL, run scripts, start API on `:5015` and Web on `:5285`)
- [ ] Dockerfile / docker-compose (API + Web + PostgreSQL)
