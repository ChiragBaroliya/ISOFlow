# ISOFlow Database Schema Documentation

This directory contains the PostgreSQL schema, stored procedures/functions, seed data and migrations for the **ISOFlow ISO Compliance Management Platform**. `ISOFlow.Api` accesses it through Dapper repositories in `ISOFlow.Infrastructure/Repositories`.

---

## 📁 Directory Structure

```
ISOFlow.Domain/DatabaseSchema/
├── 01_Tables_And_Constraints.sql            # DROPs and recreates all 24 tables, keys, constraints and indexes
├── 02_Stored_Procedures.sql                 # sp_* functions: CRUD, paged lists, SoA, risk heat map, related counts
├── 03_Functions.sql                         # fn_* helpers: risk score/level, overdue tasks, standard compliance summary
├── 04_Seed_Data.sql                         # DEV ONLY: demo organizations, users, standards and the golden-scenario chain
├── 05_Migration_Security_And_Snapshots.sql  # One-time upgrade for databases created before password hashing / compliance_snapshots
└── DatabaseSchemaReadme.md
```

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

Run the scripts in order with `psql`. `01` **drops all existing tables**, so only run it against a database you intend to reset.

```bash
psql -h localhost -U postgres -c "CREATE DATABASE isoflow;"

psql -h localhost -U postgres -d isoflow -v ON_ERROR_STOP=1 -f 01_Tables_And_Constraints.sql
psql -h localhost -U postgres -d isoflow -v ON_ERROR_STOP=1 -f 02_Stored_Procedures.sql
psql -h localhost -U postgres -d isoflow -v ON_ERROR_STOP=1 -f 03_Functions.sql
psql -h localhost -U postgres -d isoflow -v ON_ERROR_STOP=1 -f 04_Seed_Data.sql   # development only
```

Then point `ConnectionStrings:DefaultConnection` in `ISOFlow.Api` at it (use User Secrets or an environment variable for the password).

**Seed accounts (development only):** `superadmin@isoflow.com`, `alex.morgan@acme.com`, `sarah.chen@acme.com`, all with password `Test@123` (stored hashed). Never run `04_Seed_Data.sql` against production.

### Upgrading an existing database

A database created from an older version of `01_Tables_And_Constraints.sql` needs `05_Migration_Security_And_Snapshots.sql` once (it is idempotent):

```bash
psql -h localhost -U postgres -d isoflow -v ON_ERROR_STOP=1 -f 05_Migration_Security_And_Snapshots.sql
```

It converts plain-text `Test@123` passwords to their hash, adds `reset_token_expires_at`, invalidates old reset tokens, removes `sp_user_validate_login`, and creates `compliance_snapshots`. It lists any user whose password is still not hashed; those users must use *Forgot password*.

### Integration-test database

The test suite rebuilds its own database from scripts `01`–`04` on every run. Create a dedicated database whose name contains `test` and set `ISOFLOW_TEST_DB`:

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
