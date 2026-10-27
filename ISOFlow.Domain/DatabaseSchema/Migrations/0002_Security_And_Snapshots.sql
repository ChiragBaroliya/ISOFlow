-- ============================================================================
-- ISOFlow Platform - Migration 0002: password hashing, reset-token expiry, compliance snapshots
-- Compatible with: PostgreSQL 12+ / Azure PostgreSQL / Supabase
--
-- Upgrades databases created before these changes (pre-DbUp databases are baselined at 0001 and
-- then receive this script). On a fresh database every statement is a no-op. Idempotent.
-- ============================================================================

-- 1. users.password_hash: no plain-text default.
ALTER TABLE users ALTER COLUMN password_hash DROP DEFAULT;

-- Convert rows still holding the old plain-text dev password to its PBKDF2 hash (same password).
UPDATE users
SET password_hash = '$pbkdf2-sha256$i=100000$1o9Ffz/fyVm/MoK43T/Dhw==$DtlEkeJ+6+uIZT9bhNBdccstXbsQamEPeyhap2pbr5I='
WHERE password_hash = 'Test@123';

-- Any other non-hashed value can no longer log in (plain-text fallback was removed).
-- Those users must use "Forgot password". List them:
SELECT id, email AS users_requiring_password_reset
FROM users
WHERE password_hash NOT LIKE '$pbkdf2-sha256$%';

-- 2. Reset tokens: store a SHA-256 digest with an expiry. Old plain-text tokens are invalidated.
ALTER TABLE users ALTER COLUMN reset_token TYPE VARCHAR(128);
ALTER TABLE users ADD COLUMN IF NOT EXISTS reset_token_expires_at TIMESTAMP WITH TIME ZONE NULL;
UPDATE users SET reset_token = NULL, reset_token_expires_at = NULL WHERE reset_token IS NOT NULL;

-- 3. Login validation moved to the application (PBKDF2 verify).
DROP FUNCTION IF EXISTS sp_user_validate_login(VARCHAR, VARCHAR);

-- 4. Compliance snapshots for real dashboard trend history.
CREATE TABLE IF NOT EXISTS compliance_snapshots (
    id SERIAL PRIMARY KEY,
    organization_id INT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    snapshot_month DATE NOT NULL,
    compliance_score DOUBLE PRECISION NOT NULL,
    open_risks_count INT NOT NULL,
    captured_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_compliance_snapshots_org_month
    ON compliance_snapshots ((COALESCE(organization_id, 0)), snapshot_month);
