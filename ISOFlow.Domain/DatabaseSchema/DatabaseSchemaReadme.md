# ISOFlow Database Schema Documentation

This directory contains the complete T-SQL relational database schema, stored procedures, database functions, and seed data for the **ISOFlow ISO Compliance Management Platform**.

---

## 📁 Directory Structure

```
ISOFlow.Domain/DatabaseSchema/
├── 01_Tables_And_Constraints.sql   # DDL: 22 Tables, Primary Keys, Foreign Keys, Indexes, Check Constraints
├── 02_Stored_Procedures.sql        # Full CRUD, Paged Lists (OFFSET/FETCH), SoA, HeatMap, Traceability
├── 03_Functions.sql                # Scalar & Table-Valued Functions (Risk Score, Level, Summary)
├── 04_Seed_Data.sql                # Pre-seeded ISO Standards, Baseline Controls, SuperAdmin & Users
└── DatabaseSchemaReadme.md         # Schema architecture, ERD overview, and usage instructions
```

---

## 🏛️ Schema Architecture & Key Rules

1. **Zero Inline Queries**:
   - All data operations are executed strictly via Stored Procedures (`sp_*`) or Functions (`fn_*`).
2. **Database-Side Pagination & Filtering**:
   - Every list procedure takes `@PageNumber`, `@PageSize`, `@SearchTerm`, filters, and dynamic sorting, returning rows with windowed `COUNT(*) OVER() AS TotalCount`.
3. **Annex A Pre-seeded Protection**:
   - Official standards (`ISO 27001:2022`, `ISO 9001:2015`, `ISO 14001:2015`) have `IsPreseeded = 1`. Deletion is prohibited by stored procedure logic.
4. **Traceability Cross-Linking**:
   - Requirements, Controls, Risks, Policies, Processes, Tasks, Evidence, Audits, Findings, and CAPAs are linked via indexed relational foreign keys and cross-reference keys.

---

## 🚀 Execution & Deployment Order

To set up the database in SQL Server or Azure SQL Database:

```sql
-- 1. Execute Tables and Constraints
:r 01_Tables_And_Constraints.sql

-- 2. Execute Stored Procedures
:r 02_Stored_Procedures.sql

-- 3. Execute Functions
:r 03_Functions.sql

-- 4. Execute Seed Data
:r 04_Seed_Data.sql
```

---

## 📊 Stored Procedure Usage Examples

### 1. Paged Standards Query
```sql
EXEC [dbo].[sp_Standard_GetPagedList]
    @PageNumber = 1,
    @PageSize = 10,
    @SearchTerm = 'ISO',
    @StatusFilter = 'Active',
    @SortColumn = 'Code',
    @SortDirection = 'ASC';
```

### 2. Paged Controls Query with Category Filter
```sql
EXEC [dbo].[sp_Control_GetPagedList]
    @PageNumber = 1,
    @PageSize = 20,
    @SearchTerm = 'Access',
    @CategoryFilter = 'Organizational Controls',
    @StatusFilter = NULL;
```

### 3. Generate Statement of Applicability (SoA)
```sql
EXEC [dbo].[sp_GenerateStatementOfApplicability];
```

### 4. 5x5 Risk Heat Map Matrix
```sql
EXEC [dbo].[sp_GetRiskHeatMapMatrix];
```
