# ISOFlow Database Schema Documentation

This directory contains the PostgreSQL schema, stored procedures/functions, seed data and migrations for the **ISOFlow ISO Compliance Management Platform**. `ISOFlow.Api` accesses it through Dapper repositories in `ISOFlow.Infrastructure/Repositories`.

The scripts are embedded into `ISOFlow.Infrastructure` and applied by **`DatabaseMigrator`** (DbUp). You don't run them by hand.

---

## 📁 Directory Structure

```
ISOFlow.Domain/DatabaseSchema/
├── Migrations/                          # Run ONCE each, in name order; recorded in the schemaversions table
│   ├── 0001_Initial_Schema.sql          #   all 24 tables, keys, constraints, indexes, audit-log triggers (no DROPs)
│   └── 0002_Security_And_Snapshots.sql  #   password hashing, reset-token expiry, compliance_snapshots (no-op on fresh DBs)
├── Programmability/                     # Re-applied on EVERY run (CREATE OR REPLACE), so they always match the code
│   ├── 0001_Stored_Procedures.sql       #   sp_*: CRUD, paged lists, SoA, risk heat map, related counts
│   └── 0002_Functions.sql               #   fn_*: risk score/level, overdue tasks, standard compliance summary
├── Seed/                                # DEV ONLY, run once, never outside the Development environment
│   └── 0001_Demo_Data.sql               #   demo organizations, users, standards and the golden-scenario chain
└── DatabaseSchemaReadme.md
```

### Changing the schema

- **Never edit a released migration.** Add the next numbered file to `Migrations/` (e.g. `0003_Add_File_Storage.sql`). It is picked up automatically.
- Change procedures and functions in place in `Programmability/`. They're re-applied on every run. A `CREATE OR REPLACE` can't change a function's return columns; for that, add a `DROP FUNCTION IF EXISTS …` before it.
- Migrations must be safe to run inside a transaction and must not depend on seed data.

---

## 🏛️ Schema Rules

1. **Multi-tenant**: every tenant-owned table has `organization_id`. Read procedures take `p_organization_id DEFAULT NULL` (NULL = all tenants, SuperAdmin only); write procedures require it and only touch rows of that organization.
2. **Database-side pagination & filtering**: `*_get_paged` procedures take page number/size plus search and filter parameters, and return `COUNT(*) OVER() AS total_count` on every row.
3. **Pre-seeded standard protection**: official standards are flagged `is_preseeded = TRUE`; the delete procedure refuses to remove them.
4. **Traceability links** are stored two ways:
   - Foreign-key columns (e.g. `risks.control_id`, `findings.audit_id`, `capas.finding_id`).
   - JSON text arrays of **codes** (e.g. `policies.linked_control_ids_json = '["CTRL-001"]'`, `audits.check_list_control_ids_json`). The CRUD procedures don't carry these columns; repositories save and load them through `BaseRepository.SaveJsonListsAsync` / `LoadJsonListsAsync`.
5. **Credentials are never stored in plain text**: `users.password_hash` holds a PBKDF2 hash, `users.reset_token` holds a SHA-256 digest with an expiry in `reset_token_expires_at`, and `refresh_tokens.token_hash` holds a hash.
6. **Append-only audit log**: triggers on `audit_logs` reject UPDATE and DELETE.

Not every query goes through a procedure: some repositories use parameterized inline SQL (e.g. users, processes, traceability, compliance snapshots).

---

## 🚀 Setting Up a Database (PostgreSQL 12+)

### Development

Set the connection string once, then just start the API:

```bash
cd ISOFlow.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=isoflow;Username=postgres;Password=<password>"
dotnet run
```

`appsettings.Development.json` enables `Database:ApplyMigrationsOnStartup`, `CreateIfMissing` and `SeedDemoData`. On first start, the API creates the `isoflow` database, applies all migrations and procedures, and loads the demo data.

**Seed accounts (development only):** `superadmin@isoflow.com`, `alex.morgan@acme.com`, `sarah.chen@acme.com`, all with password `Test@123` (stored hashed).

### Production / other environments

Startup migrations are **off** by default. Apply them as a deploy step, before starting the new version:

```bash
ConnectionStrings__DefaultConnection="Host=…;Database=isoflow;Username=…;Password=…" \
  dotnet ISOFlow.Api.dll --migrate
```

`--migrate` applies pending migrations, re-applies procedures and functions, and exits. It doesn't need the JWT secret. Demo seed data is **never** applied outside the Development environment, even if `Database:SeedDemoData` is set.

| Setting | Default | Development |
|---|---|---|
| `Database:ApplyMigrationsOnStartup` | `false` | `true` |
| `Database:CreateIfMissing` | `false` | `true` |
| `Database:SeedDemoData` | `false` | `true` |

### Databases created before DbUp

A database built with the old hand-run scripts has tables but no `schemaversions` journal. On its first migration run, `0001` is recorded as applied **without running**, and `0002` then upgrades it. That upgrade converts plain-text `Test@123` passwords to their hash, adds `reset_token_expires_at`, invalidates old reset tokens, removes `sp_user_validate_login`, and creates `compliance_snapshots`. No data is lost. Users whose password was some other plain-text value must use *Forgot password*.

### Integration-test database

The test suite resets its own database (drops the `public` schema) and rebuilds it with `DatabaseMigrator` on every run. Create a dedicated database whose name contains `test` and set `ISOFLOW_TEST_DB`:

```bash
psql -h localhost -U postgres -c "CREATE DATABASE isoflow_test;"
export ISOFLOW_TEST_DB="Host=localhost;Port=5432;Database=isoflow_test;Username=postgres;Password=<password>"
dotnet test ISOFlow.slnx
```

Without `ISOFLOW_TEST_DB` the database tests are skipped, not failed.

---

## 📊 Procedure Usage Examples

```sql
-- Page 1 of standards for organization 1, searching "ISO"
SELECT * FROM sp_standards_get_paged(1, 10, 1, 'ISO', NULL);

-- Statement of Applicability for organization 1
SELECT * FROM sp_controls_get_soa(1);

-- 5 × 5 risk heat map for organization 1
SELECT * FROM sp_get_risk_heatmap_matrix(1);

-- Risk score helper
SELECT fn_calculate_risk_score(4, 5);
```
