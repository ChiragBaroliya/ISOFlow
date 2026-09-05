-- ============================================================================
-- ISOFlow Platform - Complete Enterprise Database Seed Data
-- Compatible with: PostgreSQL 12+ / Azure PostgreSQL / Supabase
-- Schema Version: 3.0.0 (Multi-Tenant: every tenant-owned row stamped with organization_id)
--
-- NOTE: The entire golden-scenario demo (standards, requirements, controls,
-- risks, tasks, evidence, audits, findings, CAPAs, reviews, improvements) is
-- deliberately seeded entirely under organization_id = 1 (Acme Financial
-- Technologies), matching the existing demo narrative end-to-end.
-- CyberShield (2) and Nexus Health (3) exist as organizations with zero
-- operational data and zero standards/requirements of their own - seeding a
-- second tenant's worth of demo data (its own standards library plus a small
-- set of Controls/Risks/Tasks) is an explicit follow-up, not part of this
-- change.
-- ============================================================================

-- 1. Organizations
INSERT INTO organizations (id, code, name, industry, employees, locations_json, primary_standard, status, compliance_percentage, contact_email)
OVERRIDING SYSTEM VALUE
VALUES
(1, 'ACME-CORP', 'Acme Financial Technologies', 'Fintech & Cloud Payments', 450, '["Austin HQ","London Branch","Singapore DC"]', 'ISO/IEC 27001:2022', 'Active', 87.5, 'compliance@acme.com'),
(2, 'CYBER-SHIELD', 'CyberShield Global Inc', 'Managed Security Services', 280, '["New York","San Francisco","Frankfurt"]', 'ISO/IEC 27001:2022', 'Active', 92.0, 'audit@cybershield.com'),
(3, 'NEXUS-HEALTH', 'Nexus Health Systems', 'Healthcare & Life Sciences', 1200, '["Chicago","Boston","Toronto"]', 'ISO 9001:2015', 'Active', 79.5, 'governance@nexushealth.org')
ON CONFLICT (id) DO UPDATE SET
    code = EXCLUDED.code, name = EXCLUDED.name, industry = EXCLUDED.industry,
    employees = EXCLUDED.employees, locations_json = EXCLUDED.locations_json,
    primary_standard = EXCLUDED.primary_standard, status = EXCLUDED.status,
    compliance_percentage = EXCLUDED.compliance_percentage, contact_email = EXCLUDED.contact_email;

-- 2. Users
INSERT INTO users (id, organization_id, name, email, password_hash, system_role, role, department, location, bio)
OVERRIDING SYSTEM VALUE
VALUES
(1, NULL, 'Global Platform Admin', 'superadmin@isoflow.com', 'Test@123', 0, 'Super Administrator', 'Platform Engineering', 'Austin HQ', 'Global multi-tenant system administrator'),
(2, 1, 'Alex Morgan', 'alex.morgan@acme.com', 'Test@123', 1, 'Chief Information Security Officer (CISO)', 'Information Security', 'Austin HQ', 'Certified CISM/CISSP leading ISO 27001 and SOC 2 certification programs.'),
(3, 1, 'Sarah Chen', 'sarah.chen@acme.com', 'Test@123', 2, 'Lead Compliance Manager', 'Governance & Risk', 'Austin HQ', 'ISO 27001 Lead Auditor managing control operations.')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, name = EXCLUDED.name, email = EXCLUDED.email,
    password_hash = EXCLUDED.password_hash, system_role = EXCLUDED.system_role, role = EXCLUDED.role,
    department = EXCLUDED.department, location = EXCLUDED.location, bio = EXCLUDED.bio;

-- 3. Standards (organization_id = 1, tenant-owned per-org clause library)
INSERT INTO standards (id, organization_id, code, name, revision, description, requirement_count, compliance_percentage, is_preseeded, status)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'ISO-27001-2022', 'Information Security, Cybersecurity and Privacy Protection', '2022', 'International standard specifying requirements for establishing, implementing, maintaining and continually improving an information security management system (ISMS).', 93, 87.5, TRUE, 'Active'),
(2, 1, 'ISO-9001-2015', 'Quality Management Systems (QMS)', '2015', 'International standard specifying requirements for quality management systems.', 65, 82.0, TRUE, 'Active'),
(3, 1, 'ISO-14001-2015', 'Environmental Management Systems (EMS)', '2015', 'International standard specifying requirements for environmental management systems.', 48, 91.0, TRUE, 'Active')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, name = EXCLUDED.name, revision = EXCLUDED.revision,
    description = EXCLUDED.description, requirement_count = EXCLUDED.requirement_count,
    compliance_percentage = EXCLUDED.compliance_percentage, is_preseeded = EXCLUDED.is_preseeded, status = EXCLUDED.status;

-- 4. Requirements
INSERT INTO requirements (id, organization_id, standard_id, clause, title, description, category, compliance_percentage, related_control_ids_json)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 1, 'A.5.18', 'Access Rights', 'Access rights to information and other associated assets shall be provisioned, reviewed, modified and removed in accordance with the organization-specific topic-specific policy on access control.', 'Organizational Controls', 85.0, '["CTRL-001"]'),
(2, 1, 1, 'A.5.1', 'Policies for Information Security', 'Information security policy and topic-specific policies shall be defined, approved by management, published, communicated to and acknowledged by relevant personnel.', 'Organizational Controls', 100.0, '["CTRL-002"]'),
(3, 1, 1, 'A.5.15', 'Access Control', 'Rules to control physical and logical access to information and other associated assets shall be established and implemented based on business and information security requirements.', 'Organizational Controls', 80.0, '["CTRL-001"]'),
(4, 1, 1, 'A.8.24', 'Use of Cryptography', 'Rules for the effective use of cryptography, including cryptographic key management, shall be defined and implemented.', 'Technological Controls', 90.0, '["CTRL-003"]'),
(5, 1, 1, 'A.8.8', 'Management of Technical Vulnerabilities', 'Information about technical vulnerabilities of information systems being used shall be obtained, evaluated, and appropriate measures taken.', 'Technological Controls', 75.0, '["CTRL-004"]')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, standard_id = EXCLUDED.standard_id, clause = EXCLUDED.clause, title = EXCLUDED.title,
    description = EXCLUDED.description, category = EXCLUDED.category,
    compliance_percentage = EXCLUDED.compliance_percentage, related_control_ids_json = EXCLUDED.related_control_ids_json;

-- 5. Controls
INSERT INTO controls (id, organization_id, code, title, standard_id, requirement_id, category, description, status, owner, compliance_percentage, is_applicable, justification)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'CTRL-001', 'User Access Management', 1, 1, 'Organizational Controls', 'Formal user registration, de-registration, and periodic access reviews ensuring least privilege access across all cloud and on-premise infrastructure.', 2, 'Sarah Chen', 85.0, TRUE, 'Core control for A.5.18 and zero-trust security mandate.'),
(2, 1, 'CTRL-002', 'Policies for Information Security', 1, 2, 'Organizational Controls', 'Information security policy suite reviewed annually and approved by executive management.', 2, 'Sarah Chen', 100.0, TRUE, 'Foundational ISMS requirement.'),
(3, 1, 'CTRL-003', 'Use of Cryptography', 1, 4, 'Technological Controls', 'Mandatory AES-256 encryption at rest and TLS 1.3 in transit with automated KMS key rotation.', 2, 'Alex Morgan', 90.0, TRUE, 'Required for payment data and ISO 27001 Annex A.8.24 compliance.'),
(4, 1, 'CTRL-004', 'Vulnerability Management', 1, 5, 'Technological Controls', 'Continuous automated container and host vulnerability scanning with SLA-driven patching.', 1, 'Alex Morgan', 75.0, TRUE, 'Required for threat mitigation under A.8.8.')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, standard_id = EXCLUDED.standard_id,
    requirement_id = EXCLUDED.requirement_id, category = EXCLUDED.category,
    description = EXCLUDED.description, status = EXCLUDED.status, owner = EXCLUDED.owner,
    compliance_percentage = EXCLUDED.compliance_percentage, is_applicable = EXCLUDED.is_applicable,
    justification = EXCLUDED.justification;

-- 6. Risks
INSERT INTO risks (id, organization_id, code, title, description, asset, department, owner, likelihood, impact, control_id, status)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'RISK-001', 'Unauthorized Access & Privilege Escalation', 'Risk of former employees or unprivileged users gaining unauthorized access to production financial databases.', 'Production Database Cluster', 'Engineering & Security', 'Alex Morgan', 4, 4, 1, 'Open'),
(2, 1, 'RISK-002', 'Data Exfiltration via Unencrypted Backups', 'Potential leak of sensitive customer data if disaster recovery backup snapshots lack encryption.', 'Cloud S3 Backup Storage', 'Infrastructure Operations', 'Sarah Chen', 2, 5, 3, 'Mitigated'),
(3, 1, 'RISK-003', 'Zero-Day Vulnerability Exploitation', 'Public exploitation of unpatched dependencies in payment gateway services.', 'API Payment Gateway', 'Core Engineering', 'Alex Morgan', 3, 4, 4, 'Open')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, description = EXCLUDED.description,
    asset = EXCLUDED.asset, department = EXCLUDED.department, owner = EXCLUDED.owner,
    likelihood = EXCLUDED.likelihood, impact = EXCLUDED.impact, control_id = EXCLUDED.control_id, status = EXCLUDED.status;

-- 7. Risk Treatments
INSERT INTO risk_treatments (id, organization_id, risk_id, option, treatment_plan, owner, target_date, residual_likelihood, residual_impact, status)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 1, 'Mitigate', 'Implement automated Joiner-Mover-Leaver (JML) integration between Workday HR and Okta SCIM to auto-revoke privileges upon departure.', 'Alex Morgan', '2026-11-30 00:00:00', 1, 3, 'In Progress'),
(2, 1, 2, 'Mitigate', 'Enforce AWS KMS customer-managed key encryption on all backup snapshots with mandatory SCP deny rules.', 'Sarah Chen', '2026-08-15 00:00:00', 1, 2, 'Completed')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, risk_id = EXCLUDED.risk_id, option = EXCLUDED.option, treatment_plan = EXCLUDED.treatment_plan,
    owner = EXCLUDED.owner, target_date = EXCLUDED.target_date, residual_likelihood = EXCLUDED.residual_likelihood,
    residual_impact = EXCLUDED.residual_impact, status = EXCLUDED.status;

UPDATE risks SET treatment_id = 1 WHERE id = 1;
UPDATE risks SET treatment_id = 2 WHERE id = 2;

-- 8. Policies
INSERT INTO policies (id, organization_id, code, title, version, owner, effective_date, next_review_date, status, file_path, linked_control_ids_json)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'POL-001', 'Access Control Policy', '2.1', 'Sarah Chen', '2026-01-15 00:00:00', '2027-01-15 00:00:00', 'Active', '/documents/policies/POL-001_Access_Control.pdf', '["CTRL-001"]'),
(2, 1, 'POL-002', 'Information Security Master Policy', '3.0', 'Alex Morgan', '2026-01-01 00:00:00', '2027-01-01 00:00:00', 'Active', '/documents/policies/POL-002_InfoSec_Master.pdf', '["CTRL-002"]'),
(3, 1, 'POL-003', 'Cryptographic Controls Policy', '1.4', 'Alex Morgan', '2026-03-01 00:00:00', '2027-03-01 00:00:00', 'Active', '/documents/policies/POL-003_Cryptography.pdf', '["CTRL-003"]')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, version = EXCLUDED.version,
    owner = EXCLUDED.owner, effective_date = EXCLUDED.effective_date,
    next_review_date = EXCLUDED.next_review_date, status = EXCLUDED.status,
    file_path = EXCLUDED.file_path, linked_control_ids_json = EXCLUDED.linked_control_ids_json;

-- 9. Processes
INSERT INTO processes (id, organization_id, code, title, category, owner, description, version, status, steps_json, policy_id, control_ids_json)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'PROC-001', 'Joiner-Mover-Leaver (JML) Process', 'Identity & Access', 'Sarah Chen', 'End-to-end lifecycle process covering role provisioning on hiring, privilege recalculation on department transfer, and instant de-provisioning upon termination.', '2.0', 'Active', '["HR triggers onboarding/offboarding ticket","Identity team provisions Okta role profile","Manager approves elevated privileges via Slack/Jira","Quarterly access attestation audit executed","Automated deactivation on termination timestamp"]', 1, '["CTRL-001"]'),
(2, 1, 'PROC-002', 'Cryptographic Key Lifecycle Process', 'Security Operations', 'Alex Morgan', 'Procedures for generating, distributing, storing, rotating, and revoking cryptographic keys in AWS KMS and HashiCorp Vault.', '1.2', 'Active', '["Generate KMS key with alias","Assign IAM key policy","Rotate key annually","Monitor CloudTrail for key usage anomalies"]', 3, '["CTRL-003"]')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, category = EXCLUDED.category,
    owner = EXCLUDED.owner, description = EXCLUDED.description, version = EXCLUDED.version,
    status = EXCLUDED.status, steps_json = EXCLUDED.steps_json, policy_id = EXCLUDED.policy_id,
    control_ids_json = EXCLUDED.control_ids_json;

-- 9b. Task Templates (recurring task blueprints; frequency: 0=Daily,1=Weekly,2=Fortnightly,3=Monthly,4=HalfYearly,5=Yearly)
INSERT INTO task_templates (id, organization_id, code, title, description, frequency, default_owner, related_control_id)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'TMPL-001', 'Privileged Access Certification Review', 'Review and re-certify all privileged/admin accounts across production systems.', 3, 'Sarah Chen', 1),
(2, 1, 'TMPL-002', 'Annual Penetration Test Review', 'Commission and review results of the annual external penetration test against payment gateway services.', 5, 'Alex Morgan', 3)
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title,
    description = EXCLUDED.description, frequency = EXCLUDED.frequency,
    default_owner = EXCLUDED.default_owner, related_control_id = EXCLUDED.related_control_id;

-- 10. Task Items
INSERT INTO task_items (id, organization_id, code, title, control_id, risk_id, owner, priority, due_date, status, comments)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'TASK-2026-001', 'Review Q1 Privileged Access Accounts', 1, 1, 'Sarah Chen', 2, '2026-03-31 00:00:00', 3, 'Completed for all production jump hosts.'),
(2, 1, 'TASK-2026-002', 'Rotate Database Root Encryption Keys', 3, 2, 'Alex Morgan', 2, '2026-06-30 00:00:00', 3, 'Automated rotation verified in AWS KMS.'),
(3, 1, 'TASK-2026-003', 'Execute Q3 User Access Review', 1, 1, 'Sarah Chen', 3, '2026-09-30 00:00:00', 1, 'In progress — currently reviewing engineering GitLab permissions.'),
(4, 1, 'TASK-2026-004', 'Validate Multi-Factor Authentication Enforcement', 1, 1, 'Alex Morgan', 2, '2026-10-15 00:00:00', 0, 'Pending Okta policy roll-out for contractors.'),
(5, 1, 'TASK-2026-005', 'Run Bi-Weekly Container Vulnerability Scan', 4, 3, 'Alex Morgan', 1, '2026-09-15 00:00:00', 1, 'Trivy scanner pipeline active.')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, control_id = EXCLUDED.control_id,
    risk_id = EXCLUDED.risk_id, owner = EXCLUDED.owner, priority = EXCLUDED.priority,
    due_date = EXCLUDED.due_date, status = EXCLUDED.status, comments = EXCLUDED.comments;

-- 11. Evidence
INSERT INTO evidence (id, organization_id, code, name, type, control_id, requirement_id, task_id, uploaded_by, expiry_date, status, file_url)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'EVI-2026-001', 'Q3 User Access Review Attestation Report.pdf', 1, 1, 1, 3, 'Sarah Chen', '2027-09-30 00:00:00', 'Verified', '/evidence/2026/EVI-2026-001_Access_Review_Report.pdf'),
(2, 1, 'EVI-2026-002', 'AWS KMS Encryption Key Audit Log Export', 3, 3, 4, 2, 'Alex Morgan', '2027-06-30 00:00:00', 'Verified', '/evidence/2026/EVI-2026-002_KMS_Key_Audit.csv'),
(3, 1, 'EVI-2026-003', 'Workday to Okta SCIM Automation Architecture Approval', 2, 1, 1, 1, 'Sarah Chen', '2028-01-01 00:00:00', 'Verified', '/evidence/2026/EVI-2026-003_SCIM_Approval.pdf')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, name = EXCLUDED.name, type = EXCLUDED.type,
    control_id = EXCLUDED.control_id, requirement_id = EXCLUDED.requirement_id,
    task_id = EXCLUDED.task_id, uploaded_by = EXCLUDED.uploaded_by,
    expiry_date = EXCLUDED.expiry_date, status = EXCLUDED.status, file_url = EXCLUDED.file_url;

UPDATE task_items SET evidence_id = 1 WHERE id = 3;
UPDATE task_items SET evidence_id = 2 WHERE id = 2;

-- 12. Audits
INSERT INTO audits (id, organization_id, code, title, standard_id, lead_auditor, start_date, end_date, status, completion_percentage, scope)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'AUD-2026-001', 'ISO 27001:2022 Internal ISMS Audit', 1, 'Michael Torres (External Lead Auditor)', '2026-08-10 00:00:00', '2026-08-20 00:00:00', 4, 100, 'Comprehensive annual audit of all Annex A controls, Cloud Infrastructure, JML access procedures, and cryptographic keys.'),
(2, 1, 'AUD-2026-002', 'Q4 Surveillance Audit Readiness Assessment', 1, 'Sarah Chen', '2026-11-01 00:00:00', '2026-11-10 00:00:00', 1, 20, 'Pre-assessment surveillance audit for BSI accreditation body.')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, standard_id = EXCLUDED.standard_id,
    lead_auditor = EXCLUDED.lead_auditor, start_date = EXCLUDED.start_date,
    end_date = EXCLUDED.end_date, status = EXCLUDED.status,
    completion_percentage = EXCLUDED.completion_percentage, scope = EXCLUDED.scope;

-- 13. Findings
INSERT INTO findings (id, organization_id, code, title, audit_id, requirement_id, control_id, severity, status, description, root_cause, identified_date, auditor)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'FIND-001', 'Deprovisioned Employee Access Not Removed Promptly', 1, 1, 1, 3, 2, 'During sample testing, access for 2 contractor accounts remained active in AWS console 72 hours post contract termination.', 'Manual Jira ticketing disconnect between HR and DevOps during offboarding peak.', '2026-08-15 00:00:00', 'Michael Torres'),
(2, 1, 'FIND-002', 'Information Security Policy Acknowledgement Lacked Tracking', 1, 2, 2, 1, 3, '12% of new joiners had not submitted digital acknowledgement for the 2026 security policy within 30 days.', 'Onboarding checklist lacked automated reminder notifications.', '2026-08-16 00:00:00', 'Michael Torres')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, audit_id = EXCLUDED.audit_id,
    requirement_id = EXCLUDED.requirement_id, control_id = EXCLUDED.control_id,
    severity = EXCLUDED.severity, status = EXCLUDED.status,
    description = EXCLUDED.description, root_cause = EXCLUDED.root_cause,
    identified_date = EXCLUDED.identified_date, auditor = EXCLUDED.auditor;

-- 14. CAPAs
INSERT INTO capas (id, organization_id, code, finding_id, title, root_cause, corrective_action, owner, due_date, status, effectiveness_verification)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'CAPA-001', 1, 'Automate JML Access De-provisioning via Okta-Workday Sync', 'Manual offboarding workflow caused lag between HR status change and cloud access revocation.', 'Deploy Okta SCIM real-time webhooks directly connected to Workday employment status changes. All AWS IAM roles auto-disabled immediately.', 'Alex Morgan', '2026-10-31 00:00:00', 3, 'Audit logs verified: 100% of October departures deprovisioned within 60 seconds.')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, finding_id = EXCLUDED.finding_id, title = EXCLUDED.title,
    root_cause = EXCLUDED.root_cause, corrective_action = EXCLUDED.corrective_action,
    owner = EXCLUDED.owner, due_date = EXCLUDED.due_date, status = EXCLUDED.status,
    effectiveness_verification = EXCLUDED.effectiveness_verification;

UPDATE findings SET capa_id = 1 WHERE id = 1;

-- 15. Capa Action Items
INSERT INTO capa_action_items (id, organization_id, capa_id, title, assigned_to, due_date, is_completed)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 1, 'Configure Workday outbound offboarding webhook endpoint', 'Alex Morgan', '2026-09-15 00:00:00', TRUE),
(2, 1, 1, 'Deploy AWS EventBridge Lambda function for instant IAM session revocation', 'Sarah Chen', '2026-09-30 00:00:00', TRUE),
(3, 1, 1, 'Run simulation test with dummy contractor termination and verify zero lingering credentials', 'Alex Morgan', '2026-10-15 00:00:00', FALSE)
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, capa_id = EXCLUDED.capa_id, title = EXCLUDED.title, assigned_to = EXCLUDED.assigned_to,
    due_date = EXCLUDED.due_date, is_completed = EXCLUDED.is_completed;

-- 16. Management Reviews
INSERT INTO management_reviews (id, organization_id, code, title, period, review_date, chair_person, attendees_json, summary, status)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'MR-Q4-2026', 'Executive ISMS Management Review — Q4 2026', 'Q4 2026', '2026-08-25 00:00:00', 'Alex Morgan (CISO)', '["Alex Morgan (CISO)","Sarah Chen (Lead Auditor)","David Miller (VP Engineering)","Lisa Chang (General Counsel)"]', 'Reviewed findings of AUD-2026-001, confirmed CAPA-001 progress, approved 2027 ISMS budget increase, and authorized JML automated orchestration initiative.', 'Completed')
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, period = EXCLUDED.period,
    review_date = EXCLUDED.review_date, chair_person = EXCLUDED.chair_person,
    attendees_json = EXCLUDED.attendees_json, summary = EXCLUDED.summary, status = EXCLUDED.status;

-- 17. Improvements
INSERT INTO improvements (id, organization_id, code, title, current_state, future_state, source, expected_benefit, owner, status, related_review_id, related_finding_id)
OVERRIDING SYSTEM VALUE
VALUES
(1, 1, 'IMP-001', 'Access Lifecycle Automation & Zero-Trust Posture', 'Semi-automated access reviews with potential offboarding latency.', 'Fully automated identity lifecycle with zero-standing-privileges (JIT access) across AWS, Kubernetes, and corporate SaaS.', 3, 'Zero lingering access risk, 95% reduction in manual audit preparation hours, seamless ISO 27001 recertification.', 'Sarah Chen', 2, 1, 1),
(2, 1, 'IMP-002', 'Continuous Policy Compliance Portal', 'Annual static policy PDFs distributed via email.', 'Self-service interactive policy center with automated read-receipts and quiz-based comprehension validation.', 0, '100% verifiable policy comprehension across all 450 employees.', 'Sarah Chen', 1, 1, 2)
ON CONFLICT (id) DO UPDATE SET
    organization_id = EXCLUDED.organization_id, code = EXCLUDED.code, title = EXCLUDED.title, current_state = EXCLUDED.current_state,
    future_state = EXCLUDED.future_state, source = EXCLUDED.source,
    expected_benefit = EXCLUDED.expected_benefit, owner = EXCLUDED.owner,
    status = EXCLUDED.status, related_review_id = EXCLUDED.related_review_id,
    related_finding_id = EXCLUDED.related_finding_id;

-- Ensure sequence values are higher than seeded IDs
SELECT setval(pg_get_serial_sequence('organizations', 'id'), COALESCE(MAX(id), 1)) FROM organizations;
SELECT setval(pg_get_serial_sequence('users', 'id'), COALESCE(MAX(id), 1)) FROM users;
SELECT setval(pg_get_serial_sequence('standards', 'id'), COALESCE(MAX(id), 1)) FROM standards;
SELECT setval(pg_get_serial_sequence('requirements', 'id'), COALESCE(MAX(id), 1)) FROM requirements;
SELECT setval(pg_get_serial_sequence('controls', 'id'), COALESCE(MAX(id), 1)) FROM controls;
SELECT setval(pg_get_serial_sequence('risks', 'id'), COALESCE(MAX(id), 1)) FROM risks;
SELECT setval(pg_get_serial_sequence('risk_treatments', 'id'), COALESCE(MAX(id), 1)) FROM risk_treatments;
SELECT setval(pg_get_serial_sequence('policies', 'id'), COALESCE(MAX(id), 1)) FROM policies;
SELECT setval(pg_get_serial_sequence('processes', 'id'), COALESCE(MAX(id), 1)) FROM processes;
SELECT setval(pg_get_serial_sequence('task_templates', 'id'), COALESCE(MAX(id), 1)) FROM task_templates;
SELECT setval(pg_get_serial_sequence('task_items', 'id'), COALESCE(MAX(id), 1)) FROM task_items;
SELECT setval(pg_get_serial_sequence('evidence', 'id'), COALESCE(MAX(id), 1)) FROM evidence;
SELECT setval(pg_get_serial_sequence('audits', 'id'), COALESCE(MAX(id), 1)) FROM audits;
SELECT setval(pg_get_serial_sequence('findings', 'id'), COALESCE(MAX(id), 1)) FROM findings;
SELECT setval(pg_get_serial_sequence('capas', 'id'), COALESCE(MAX(id), 1)) FROM capas;
SELECT setval(pg_get_serial_sequence('capa_action_items', 'id'), COALESCE(MAX(id), 1)) FROM capa_action_items;
SELECT setval(pg_get_serial_sequence('management_reviews', 'id'), COALESCE(MAX(id), 1)) FROM management_reviews;
SELECT setval(pg_get_serial_sequence('improvements', 'id'), COALESCE(MAX(id), 1)) FROM improvements;
