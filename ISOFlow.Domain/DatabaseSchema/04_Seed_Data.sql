-- ============================================================================
-- ISOFlow Platform - Database Seed Data
-- Compatible with: PostgreSQL 12+ / Azure PostgreSQL / Supabase
-- Schema Version: 2.0.0 (Integer Primary Keys)
-- ============================================================================

-- 1. Seed Organization
INSERT INTO organizations (id, code, name, industry, employees, locations_json, primary_standard, status, compliance_percentage, contact_email)
OVERRIDING SYSTEM VALUE
VALUES (1, 'ACME-CORP', 'Acme Financial Technologies', 'Fintech & Cloud Payments', 450, '["Austin HQ","London Branch","Singapore DC"]', 'ISO/IEC 27001:2022', 'Active', 87.5, 'compliance@acme.com')
ON CONFLICT (id) DO NOTHING;

-- 2. Seed Default Users
INSERT INTO users (id, organization_id, name, email, password_hash, system_role, role, department, location, bio)
OVERRIDING SYSTEM VALUE
VALUES 
(1, NULL, 'Global Platform Admin', 'superadmin@isoflow.com', 'Test@123', 0, 'Super Administrator', 'Platform Engineering', 'Austin HQ', 'Global multi-tenant system administrator'),
(2, 1, 'Alex Morgan', 'alex.morgan@acme.com', 'Test@123', 1, 'Chief Information Security Officer (CISO)', 'Information Security', 'Austin HQ', 'Certified CISM/CISSP leading ISO 27001 and SOC 2 certification programs.'),
(3, 1, 'Sarah Chen', 'sarah.chen@acme.com', 'Test@123', 2, 'Lead Compliance Manager', 'Governance & Risk', 'Austin HQ', 'ISO 27001 Lead Auditor managing control operations.')
ON CONFLICT (id) DO NOTHING;

-- 3. Seed Preseeded ISO Standards
INSERT INTO standards (id, code, name, revision, description, requirement_count, compliance_percentage, is_preseeded, status)
OVERRIDING SYSTEM VALUE
VALUES
(1, 'ISO 27001:2022', 'Information Security, Cybersecurity and Privacy Protection', '2022', 'International standard for Information Security Management Systems (ISMS).', 93, 87.5, TRUE, 'Active'),
(2, 'ISO 9001:2015', 'Quality Management Systems (QMS)', '2015', 'International standard specifying requirements for quality management systems.', 65, 82.0, TRUE, 'Active'),
(3, 'ISO 14001:2015', 'Environmental Management Systems (EMS)', '2015', 'International standard specifying requirements for environmental management systems.', 48, 91.0, TRUE, 'Active')
ON CONFLICT (id) DO NOTHING;

-- 4. Seed Baseline Controls
INSERT INTO controls (id, code, title, standard_id, category, description, status, owner, compliance_percentage, is_applicable, justification)
OVERRIDING SYSTEM VALUE
VALUES
(1, 'A.5.1', 'Policies for Information Security', 1, 'Organizational Controls', 'Information security policy and topic-specific policies shall be defined, approved by management, published, communicated to and acknowledged by relevant personnel and relevant interested parties.', 3, 'Sarah Chen', 100.0, TRUE, 'Core requirement for ISMS governance.'),
(2, 'A.5.15', 'Access Control', 1, 'Organizational Controls', 'Rules to control physical and logical access to information and other associated assets shall be established and implemented based on business and information security requirements.', 2, 'David Miller', 85.0, TRUE, 'Zero-trust RBAC mandatory for all infrastructure.'),
(3, 'A.8.24', 'Use of Cryptography', 1, 'Technological Controls', 'Rules for the effective use of cryptography, including cryptographic key management, shall be defined and implemented.', 3, 'Emily Watson', 95.0, TRUE, 'AES-256 at rest and TLS 1.3 in transit enforced.')
ON CONFLICT (id) DO NOTHING;
