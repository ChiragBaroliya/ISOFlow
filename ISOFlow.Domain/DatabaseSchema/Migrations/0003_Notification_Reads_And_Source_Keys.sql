-- ============================================================================
-- ISOFlow Platform - Migration 0003: per-user notification read state, notification source keys
-- Compatible with: PostgreSQL 12+ / Azure PostgreSQL / Supabase
--
-- notifications.is_read was a single flag per organization (one user reading a notification
-- marked it read for everyone). Read state is now tracked per user in notification_reads;
-- notifications.is_read is no longer used by the application.
--
-- notifications.source_key identifies what generated a notification (e.g. 'task-overdue:42') so
-- background jobs can raise each notification at most once per organization.
-- ============================================================================

CREATE TABLE IF NOT EXISTS notification_reads (
    notification_id INT NOT NULL REFERENCES notifications(id) ON DELETE CASCADE,
    user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    read_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (notification_id, user_id)
);
CREATE INDEX IF NOT EXISTS ix_notification_reads_user_id ON notification_reads (user_id);

ALTER TABLE notifications ADD COLUMN IF NOT EXISTS source_key VARCHAR(200) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_notifications_org_source_key
    ON notifications (organization_id, source_key) WHERE source_key IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_notifications_org_created ON notifications (organization_id, created_at DESC);
