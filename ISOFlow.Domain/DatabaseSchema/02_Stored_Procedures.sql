-- ============================================================================
-- ISOFlow Platform - Complete Stored Procedures & Functions Suite
-- Compatible with: PostgreSQL 12+ / Azure PostgreSQL / Supabase
-- Zero Inline Queries: 100% of CRUD, Pagination, Search, and Filtering in Database
-- Timezone: Indian Standard Time (IST / Asia/Kolkata)
-- ============================================================================

-- ============================================================================
-- 1. STANDARDS & REQUIREMENTS
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_standards_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(200), revision VARCHAR(50),
    description TEXT, requirement_count INT, compliance_percentage DOUBLE PRECISION,
    is_preseeded BOOLEAN, status VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.code, s.name, s.revision, s.description, s.requirement_count, s.compliance_percentage, s.is_preseeded, s.status
    FROM standards s ORDER BY s.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_standards_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(200), revision VARCHAR(50),
    description TEXT, requirement_count INT, compliance_percentage DOUBLE PRECISION,
    is_preseeded BOOLEAN, status VARCHAR(50), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.code, s.name, s.revision, s.description, s.requirement_count, s.compliance_percentage, s.is_preseeded, s.status,
           COUNT(*) OVER() AS total_count
    FROM standards s
    WHERE (p_search_term IS NULL OR s.name ILIKE '%' || p_search_term || '%' OR s.code ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR s.status ILIKE p_status)
    ORDER BY s.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_standards_get_by_id(p_id VARCHAR(50))
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(200), revision VARCHAR(50),
    description TEXT, requirement_count INT, compliance_percentage DOUBLE PRECISION,
    is_preseeded BOOLEAN, status VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.id, s.code, s.name, s.revision, s.description, s.requirement_count, s.compliance_percentage, s.is_preseeded, s.status
    FROM standards s
    WHERE s.id::VARCHAR = p_id OR LOWER(s.code) = LOWER(p_id) LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_standards_create(
    p_code VARCHAR(50), p_name VARCHAR(200), p_revision VARCHAR(50), p_description TEXT,
    p_requirement_count INT, p_compliance_percentage DOUBLE PRECISION, p_is_preseeded BOOLEAN, p_status VARCHAR(50)
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO standards (code, name, revision, description, requirement_count, compliance_percentage, is_preseeded, status, created_at)
    VALUES (p_code, p_name, p_revision, p_description, p_requirement_count, p_compliance_percentage, p_is_preseeded, p_status, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_standards_update(
    p_id VARCHAR(50), p_name VARCHAR(200), p_revision VARCHAR(50), p_description TEXT,
    p_requirement_count INT, p_compliance_percentage DOUBLE PRECISION
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE standards
    SET name = p_name, revision = p_revision, description = p_description,
        requirement_count = p_requirement_count, compliance_percentage = p_compliance_percentage,
        updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_standards_delete(p_id VARCHAR(50))
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM standards WHERE (id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id)) AND is_preseeded = FALSE;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_requirements_get_by_standard_id(p_standard_id VARCHAR(50))
RETURNS TABLE (
    id INT, standard_id INT, clause VARCHAR(50), title VARCHAR(250), description TEXT,
    category VARCHAR(100), compliance_percentage DOUBLE PRECISION
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.standard_id, r.clause, r.title, r.description, r.category, r.compliance_percentage
    FROM requirements r
    WHERE r.standard_id::VARCHAR = p_standard_id 
       OR r.standard_id IN (SELECT s.id FROM standards s WHERE LOWER(s.code) = LOWER(p_standard_id))
    ORDER BY r.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_requirements_get_paged_by_standard_id(
    p_standard_id VARCHAR(50), p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL
)
RETURNS TABLE (
    id INT, standard_id INT, clause VARCHAR(50), title VARCHAR(250), description TEXT,
    category VARCHAR(100), compliance_percentage DOUBLE PRECISION, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.standard_id, r.clause, r.title, r.description, r.category, r.compliance_percentage,
           COUNT(*) OVER() AS total_count
    FROM requirements r
    WHERE (r.standard_id::VARCHAR = p_standard_id OR r.standard_id IN (SELECT s.id FROM standards s WHERE LOWER(s.code) = LOWER(p_standard_id)))
      AND (p_search_term IS NULL OR r.title ILIKE '%' || p_search_term || '%' OR r.clause ILIKE '%' || p_search_term || '%')
    ORDER BY r.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_requirements_get_by_id(p_id VARCHAR(50))
RETURNS TABLE (
    id INT, standard_id INT, clause VARCHAR(50), title VARCHAR(250), description TEXT,
    category VARCHAR(100), compliance_percentage DOUBLE PRECISION
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.standard_id, r.clause, r.title, r.description, r.category, r.compliance_percentage
    FROM requirements r
    WHERE r.id::VARCHAR = p_id OR LOWER(r.clause) = LOWER(p_id) LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_requirements_create(
    p_standard_id INT, p_clause VARCHAR(50), p_title VARCHAR(250), p_description TEXT,
    p_category VARCHAR(100), p_compliance_percentage DOUBLE PRECISION
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO requirements (standard_id, clause, title, description, category, compliance_percentage, created_at)
    VALUES (p_standard_id, p_clause, p_title, p_description, p_category, p_compliance_percentage, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_requirements_update(
    p_id VARCHAR(50), p_clause VARCHAR(50), p_title VARCHAR(250), p_description TEXT,
    p_category VARCHAR(100), p_compliance_percentage DOUBLE PRECISION
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE requirements
    SET clause = p_clause, title = p_title, description = p_description,
        category = p_category, compliance_percentage = p_compliance_percentage,
        updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(clause) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_requirements_delete(p_id VARCHAR(50))
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM requirements WHERE id::VARCHAR = p_id OR LOWER(clause) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 2. CONTROLS & STATEMENT OF APPLICABILITY (SoA)
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_controls_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), requirement_id INT, standard_id INT,
    category VARCHAR(100), description TEXT, status INT, owner VARCHAR(150),
    compliance_percentage DOUBLE PRECISION, is_applicable BOOLEAN, justification TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.code, c.title, c.requirement_id, c.standard_id, c.category, c.description,
           c.status, c.owner, c.compliance_percentage, c.is_applicable, c.justification
    FROM controls c ORDER BY c.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_controls_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL,
    p_category VARCHAR(100) DEFAULT NULL, p_status INT DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), requirement_id INT, standard_id INT,
    category VARCHAR(100), description TEXT, status INT, owner VARCHAR(150),
    compliance_percentage DOUBLE PRECISION, is_applicable BOOLEAN, justification TEXT,
    total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.code, c.title, c.requirement_id, c.standard_id, c.category, c.description,
           c.status, c.owner, c.compliance_percentage, c.is_applicable, c.justification,
           COUNT(*) OVER() AS total_count
    FROM controls c
    WHERE (p_search_term IS NULL OR c.title ILIKE '%' || p_search_term || '%' OR c.code ILIKE '%' || p_search_term || '%' OR c.owner ILIKE '%' || p_search_term || '%')
      AND (p_category IS NULL OR c.category ILIKE p_category)
      AND (p_status IS NULL OR c.status = p_status)
    ORDER BY c.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_controls_get_by_id(p_id VARCHAR(50))
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), requirement_id INT, standard_id INT,
    category VARCHAR(100), description TEXT, status INT, owner VARCHAR(150),
    compliance_percentage DOUBLE PRECISION, is_applicable BOOLEAN, justification TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.code, c.title, c.requirement_id, c.standard_id, c.category, c.description,
           c.status, c.owner, c.compliance_percentage, c.is_applicable, c.justification
    FROM controls c
    WHERE c.id::VARCHAR = p_id OR LOWER(c.code) = LOWER(p_id) LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_controls_create(
    p_code VARCHAR(50), p_title VARCHAR(250), p_category VARCHAR(100), p_description TEXT,
    p_status INT, p_owner VARCHAR(150), p_compliance_percentage DOUBLE PRECISION,
    p_is_applicable BOOLEAN, p_justification TEXT
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO controls (code, title, category, description, status, owner, compliance_percentage, is_applicable, justification, created_at)
    VALUES (p_code, p_title, p_category, p_description, p_status, p_owner, p_compliance_percentage, p_is_applicable, p_justification, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_controls_update(
    p_id VARCHAR(50), p_title VARCHAR(250), p_category VARCHAR(100), p_description TEXT,
    p_status INT, p_owner VARCHAR(150), p_compliance_percentage DOUBLE PRECISION,
    p_is_applicable BOOLEAN, p_justification TEXT
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE controls
    SET title = p_title, category = p_category, description = p_description, status = p_status,
        owner = p_owner, compliance_percentage = p_compliance_percentage, is_applicable = p_is_applicable,
        justification = p_justification, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_controls_delete(p_id VARCHAR(50))
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM controls WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_controls_get_soa()
RETURNS TABLE (
    control_id INT, control_code VARCHAR(50), control_title VARCHAR(250),
    applicable BOOLEAN, justification TEXT, implementation_status VARCHAR(50),
    owner VARCHAR(150), evidence_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id AS control_id, c.code AS control_code, c.title AS control_title,
           c.is_applicable AS applicable, COALESCE(c.justification, 'Standard baseline control requirement') AS justification,
           (CASE c.status
               WHEN 0 THEN 'Not Implemented' WHEN 1 THEN 'In Development'
               WHEN 2 THEN 'Implemented' WHEN 3 THEN 'Tested' WHEN 4 THEN 'Needs Review'
               ELSE 'Unknown'
           END)::VARCHAR(50) AS implementation_status,
           c.owner, COUNT(e.id) AS evidence_count
    FROM controls c
    LEFT JOIN evidence e ON c.id = e.control_id
    GROUP BY c.id, c.code, c.title, c.is_applicable, c.justification, c.status, c.owner
    ORDER BY c.id ASC;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 3. RISKS & RISK TREATMENTS
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_risks_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), description TEXT, asset VARCHAR(150),
    department VARCHAR(100), owner VARCHAR(150), likelihood INT, impact INT, score INT,
    treatment_id INT, control_id INT, status VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.code, r.title, r.description, r.asset, r.department, r.owner,
           r.likelihood, r.impact, (r.likelihood * r.impact) AS score,
           r.treatment_id, r.control_id, r.status
    FROM risks r ORDER BY r.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risks_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), description TEXT, asset VARCHAR(150),
    department VARCHAR(100), owner VARCHAR(150), likelihood INT, impact INT, score INT,
    treatment_id INT, control_id INT, status VARCHAR(50), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.code, r.title, r.description, r.asset, r.department, r.owner,
           r.likelihood, r.impact, (r.likelihood * r.impact) AS score,
           r.treatment_id, r.control_id, r.status, COUNT(*) OVER() AS total_count
    FROM risks r
    WHERE (p_search_term IS NULL OR r.title ILIKE '%' || p_search_term || '%' OR r.code ILIKE '%' || p_search_term || '%' OR r.asset ILIKE '%' || p_search_term || '%' OR r.owner ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR r.status ILIKE p_status)
    ORDER BY (r.likelihood * r.impact) DESC, r.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risks_get_by_id(p_id VARCHAR(50))
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), description TEXT, asset VARCHAR(150),
    department VARCHAR(100), owner VARCHAR(150), likelihood INT, impact INT, score INT,
    treatment_id INT, control_id INT, status VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.code, r.title, r.description, r.asset, r.department, r.owner,
           r.likelihood, r.impact, (r.likelihood * r.impact) AS score,
           r.treatment_id, r.control_id, r.status
    FROM risks r
    WHERE r.id::VARCHAR = p_id OR LOWER(r.code) = LOWER(p_id) LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risks_create(
    p_code VARCHAR(50), p_title VARCHAR(250), p_description TEXT, p_asset VARCHAR(150),
    p_department VARCHAR(100), p_owner VARCHAR(150), p_likelihood INT, p_impact INT,
    p_control_id INT, p_status VARCHAR(50)
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO risks (code, title, description, asset, department, owner, likelihood, impact, control_id, status, created_at)
    VALUES (p_code, p_title, p_description, p_asset, p_department, p_owner, p_likelihood, p_impact, p_control_id, p_status, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risks_update(
    p_id VARCHAR(50), p_title VARCHAR(250), p_description TEXT, p_asset VARCHAR(150),
    p_department VARCHAR(100), p_owner VARCHAR(150), p_likelihood INT, p_impact INT,
    p_control_id INT, p_status VARCHAR(50)
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE risks
    SET title = p_title, description = p_description, asset = p_asset, department = p_department,
        owner = p_owner, likelihood = p_likelihood, impact = p_impact, control_id = p_control_id,
        status = p_status, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risks_delete(p_id VARCHAR(50))
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM risks WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risk_treatments_create(
    p_risk_id INT, p_option VARCHAR(50), p_treatment_plan TEXT, p_owner VARCHAR(150),
    p_target_date TIMESTAMP WITHOUT TIME ZONE, p_residual_likelihood INT, p_residual_impact INT, p_status VARCHAR(50)
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO risk_treatments (risk_id, option, treatment_plan, owner, target_date, residual_likelihood, residual_impact, status, created_at)
    VALUES (p_risk_id, p_option, p_treatment_plan, p_owner, p_target_date, p_residual_likelihood, p_residual_impact, p_status, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;

    UPDATE risks SET treatment_id = v_id WHERE id = p_risk_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risk_treatments_get_by_risk_id(p_risk_id VARCHAR(50))
RETURNS TABLE (
    id INT, risk_id INT, option VARCHAR(50), treatment_plan TEXT, owner VARCHAR(150),
    target_date TIMESTAMP WITHOUT TIME ZONE, residual_likelihood INT, residual_impact INT, status VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT t.id, t.risk_id, t.option, t.treatment_plan, t.owner, t.target_date, t.residual_likelihood, t.residual_impact, t.status
    FROM risk_treatments t
    WHERE t.risk_id::VARCHAR = p_risk_id 
       OR t.risk_id IN (SELECT r.id FROM risks r WHERE LOWER(r.code) = LOWER(p_risk_id))
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_risk_treatments_get_paged(p_page_number INT, p_page_size INT)
RETURNS TABLE (
    id INT, risk_id INT, option VARCHAR(50), treatment_plan TEXT, owner VARCHAR(150),
    target_date TIMESTAMP WITHOUT TIME ZONE, residual_likelihood INT, residual_impact INT, status VARCHAR(50), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT t.id, t.risk_id, t.option, t.treatment_plan, t.owner, t.target_date, t.residual_likelihood, t.residual_impact, t.status,
           COUNT(*) OVER() AS total_count
    FROM risk_treatments t ORDER BY t.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 4. POLICIES & PROCESSES
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_policies_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), version VARCHAR(50), owner VARCHAR(150),
    effective_date TIMESTAMP WITHOUT TIME ZONE, next_review_date TIMESTAMP WITHOUT TIME ZONE,
    status VARCHAR(50), file_path VARCHAR(500)
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.code, p.title, p.version, p.owner, p.effective_date, p.next_review_date, p.status, p.file_path
    FROM policies p ORDER BY p.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_policies_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), version VARCHAR(50), owner VARCHAR(150),
    effective_date TIMESTAMP WITHOUT TIME ZONE, next_review_date TIMESTAMP WITHOUT TIME ZONE,
    status VARCHAR(50), file_path VARCHAR(500), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.code, p.title, p.version, p.owner, p.effective_date, p.next_review_date, p.status, p.file_path,
           COUNT(*) OVER() AS total_count
    FROM policies p
    WHERE (p_search_term IS NULL OR p.title ILIKE '%' || p_search_term || '%' OR p.code ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR p.status ILIKE p_status)
    ORDER BY p.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_policies_get_by_id(p_id VARCHAR(50))
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), version VARCHAR(50), owner VARCHAR(150),
    effective_date TIMESTAMP WITHOUT TIME ZONE, next_review_date TIMESTAMP WITHOUT TIME ZONE,
    status VARCHAR(50), file_path VARCHAR(500)
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.code, p.title, p.version, p.owner, p.effective_date, p.next_review_date, p.status, p.file_path
    FROM policies p
    WHERE p.id::VARCHAR = p_id OR LOWER(p.code) = LOWER(p_id) LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_policies_create(
    p_code VARCHAR(50), p_title VARCHAR(250), p_version VARCHAR(50), p_owner VARCHAR(150),
    p_effective_date TIMESTAMP WITHOUT TIME ZONE, p_next_review_date TIMESTAMP WITHOUT TIME ZONE,
    p_status VARCHAR(50), p_file_path VARCHAR(500)
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO policies (code, title, version, owner, effective_date, next_review_date, status, file_path, created_at)
    VALUES (p_code, p_title, p_version, p_owner, p_effective_date, p_next_review_date, p_status, p_file_path, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_policies_update(
    p_id VARCHAR(50), p_title VARCHAR(250), p_version VARCHAR(50), p_owner VARCHAR(150),
    p_effective_date TIMESTAMP WITHOUT TIME ZONE, p_next_review_date TIMESTAMP WITHOUT TIME ZONE,
    p_status VARCHAR(50), p_file_path VARCHAR(500)
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE policies
    SET title = p_title, version = p_version, owner = p_owner, effective_date = p_effective_date,
        next_review_date = p_next_review_date, status = p_status, file_path = p_file_path, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_policies_delete(p_id VARCHAR(50))
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM policies WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_processes_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL,
    p_category VARCHAR(100) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), category VARCHAR(100), owner VARCHAR(150),
    description TEXT, version VARCHAR(50), status VARCHAR(50), policy_id INT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT pr.id, pr.code, pr.title, pr.category, pr.owner, pr.description, pr.version, pr.status, pr.policy_id,
           COUNT(*) OVER() AS total_count
    FROM processes pr
    WHERE (p_search_term IS NULL OR pr.title ILIKE '%' || p_search_term || '%' OR pr.code ILIKE '%' || p_search_term || '%' OR pr.owner ILIKE '%' || p_search_term || '%')
      AND (p_category IS NULL OR pr.category ILIKE p_category)
      AND (p_status IS NULL OR pr.status ILIKE p_status)
    ORDER BY pr.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 5. TASKS & TASK TEMPLATES
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_tasks_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), control_id INT, risk_id INT, owner VARCHAR(150),
    priority INT, due_date TIMESTAMP WITHOUT TIME ZONE, status INT, evidence_id INT,
    completed_date TIMESTAMP WITHOUT TIME ZONE, comments TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT t.id, t.code, t.title, t.control_id, t.risk_id, t.owner, t.priority, t.due_date,
           t.status, t.evidence_id, t.completed_date, t.comments
    FROM task_items t ORDER BY t.due_date ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_tasks_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL,
    p_status INT DEFAULT NULL, p_owner VARCHAR(150) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), control_id INT, risk_id INT, owner VARCHAR(150),
    priority INT, due_date TIMESTAMP WITHOUT TIME ZONE, status INT, evidence_id INT,
    completed_date TIMESTAMP WITHOUT TIME ZONE, comments TEXT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT t.id, t.code, t.title, t.control_id, t.risk_id, t.owner, t.priority, t.due_date,
           t.status, t.evidence_id, t.completed_date, t.comments, COUNT(*) OVER() AS total_count
    FROM task_items t
    WHERE (p_search_term IS NULL OR t.title ILIKE '%' || p_search_term || '%' OR t.code ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR t.status = p_status)
      AND (p_owner IS NULL OR t.owner ILIKE p_owner)
    ORDER BY t.due_date ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_tasks_get_by_id(p_id VARCHAR(50))
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), control_id INT, risk_id INT, owner VARCHAR(150),
    priority INT, due_date TIMESTAMP WITHOUT TIME ZONE, status INT, evidence_id INT,
    completed_date TIMESTAMP WITHOUT TIME ZONE, comments TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT t.id, t.code, t.title, t.control_id, t.risk_id, t.owner, t.priority, t.due_date,
           t.status, t.evidence_id, t.completed_date, t.comments
    FROM task_items t
    WHERE t.id::VARCHAR = p_id OR LOWER(t.code) = LOWER(p_id) LIMIT 1;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_tasks_create(
    p_code VARCHAR(50), p_title VARCHAR(250), p_control_id INT, p_risk_id INT,
    p_owner VARCHAR(150), p_priority INT, p_due_date TIMESTAMP WITHOUT TIME ZONE,
    p_status INT, p_comments TEXT
)
RETURNS INT AS $$
DECLARE v_id INT;
BEGIN
    INSERT INTO task_items (code, title, control_id, risk_id, owner, priority, due_date, status, comments, created_at)
    VALUES (p_code, p_title, p_control_id, p_risk_id, p_owner, p_priority, p_due_date, p_status, p_comments, (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata'))
    RETURNING id INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_tasks_update(
    p_id VARCHAR(50), p_title VARCHAR(250), p_owner VARCHAR(150), p_priority INT,
    p_due_date TIMESTAMP WITHOUT TIME ZONE, p_status INT, p_comments TEXT
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE task_items
    SET title = p_title, owner = p_owner, priority = p_priority, due_date = p_due_date,
        status = p_status, comments = p_comments, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_tasks_update_status(p_id VARCHAR(50), p_status INT)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE task_items
    SET status = p_status,
        completed_date = CASE WHEN p_status = 3 THEN (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') ELSE NULL END,
        updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_tasks_delete(p_id VARCHAR(50))
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM task_items WHERE id::VARCHAR = p_id OR LOWER(code) = LOWER(p_id);
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 6. EVIDENCE VAULT
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_evidence_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(250), type INT, control_id INT,
    requirement_id INT, task_id INT, audit_id INT, uploaded_by VARCHAR(150),
    upload_date TIMESTAMP WITHOUT TIME ZONE, expiry_date TIMESTAMP WITHOUT TIME ZONE,
    status VARCHAR(50), file_url VARCHAR(500)
) AS $$
BEGIN
    RETURN QUERY
    SELECT e.id, e.code, e.name, e.type, e.control_id, e.requirement_id, e.task_id, e.audit_id,
           e.uploaded_by, e.upload_date, e.expiry_date, e.status, e.file_url
    FROM evidence e ORDER BY e.upload_date DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_evidence_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(250), type INT, control_id INT,
    requirement_id INT, task_id INT, audit_id INT, uploaded_by VARCHAR(150),
    upload_date TIMESTAMP WITHOUT TIME ZONE, expiry_date TIMESTAMP WITHOUT TIME ZONE,
    status VARCHAR(50), file_url VARCHAR(500), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT e.id, e.code, e.name, e.type, e.control_id, e.requirement_id, e.task_id, e.audit_id,
           e.uploaded_by, e.upload_date, e.expiry_date, e.status, e.file_url,
           COUNT(*) OVER() AS total_count
    FROM evidence e
    WHERE (p_search_term IS NULL OR e.name ILIKE '%' || p_search_term || '%' OR e.code ILIKE '%' || p_search_term || '%' OR e.uploaded_by ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR e.status ILIKE p_status)
    ORDER BY e.upload_date DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 7. AUDITS, FINDINGS & CAPA
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_audits_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), standard_id INT, lead_auditor VARCHAR(150),
    start_date TIMESTAMP WITHOUT TIME ZONE, end_date TIMESTAMP WITHOUT TIME ZONE, status INT,
    completion_percentage INT, scope TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.code, a.title, a.standard_id, a.lead_auditor, a.start_date, a.end_date,
           a.status, a.completion_percentage, a.scope
    FROM audits a ORDER BY a.start_date DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_audits_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status INT DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), standard_id INT, lead_auditor VARCHAR(150),
    start_date TIMESTAMP WITHOUT TIME ZONE, end_date TIMESTAMP WITHOUT TIME ZONE, status INT,
    completion_percentage INT, scope TEXT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.code, a.title, a.standard_id, a.lead_auditor, a.start_date, a.end_date,
           a.status, a.completion_percentage, a.scope, COUNT(*) OVER() AS total_count
    FROM audits a
    WHERE (p_search_term IS NULL OR a.title ILIKE '%' || p_search_term || '%' OR a.code ILIKE '%' || p_search_term || '%' OR a.lead_auditor ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR a.status = p_status)
    ORDER BY a.start_date DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_findings_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status INT DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), audit_id INT, requirement_id INT,
    control_id INT, severity INT, status INT, description TEXT, root_cause TEXT,
    identified_date TIMESTAMP WITHOUT TIME ZONE, auditor VARCHAR(150), capa_id INT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT f.id, f.code, f.title, f.audit_id, f.requirement_id, f.control_id, f.severity,
           f.status, f.description, f.root_cause, f.identified_date, f.auditor, f.capa_id,
           COUNT(*) OVER() AS total_count
    FROM findings f
    WHERE (p_search_term IS NULL OR f.title ILIKE '%' || p_search_term || '%' OR f.code ILIKE '%' || p_search_term || '%' OR f.auditor ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR f.status = p_status)
    ORDER BY f.identified_date DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_capas_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), finding_id INT, title VARCHAR(250), root_cause TEXT,
    corrective_action TEXT, owner VARCHAR(150), due_date TIMESTAMP WITHOUT TIME ZONE,
    status INT, effectiveness_verification TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.code, c.finding_id, c.title, c.root_cause, c.corrective_action,
           c.owner, c.due_date, c.status, c.effectiveness_verification
    FROM capas c ORDER BY c.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_capas_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status INT DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), finding_id INT, title VARCHAR(250), root_cause TEXT,
    corrective_action TEXT, owner VARCHAR(150), due_date TIMESTAMP WITHOUT TIME ZONE,
    status INT, effectiveness_verification TEXT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id, c.code, c.finding_id, c.title, c.root_cause, c.corrective_action,
           c.owner, c.due_date, c.status, c.effectiveness_verification, COUNT(*) OVER() AS total_count
    FROM capas c
    WHERE (p_search_term IS NULL OR c.title ILIKE '%' || p_search_term || '%' OR c.code ILIKE '%' || p_search_term || '%' OR c.owner ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR c.status = p_status)
    ORDER BY c.due_date ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 8. MANAGEMENT REVIEWS & IMPROVEMENTS
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_management_reviews_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), period VARCHAR(50),
    review_date TIMESTAMP WITHOUT TIME ZONE, chair_person VARCHAR(150),
    summary TEXT, status VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT m.id, m.code, m.title, m.period, m.review_date, m.chair_person, m.summary, m.status
    FROM management_reviews m ORDER BY m.review_date DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_management_reviews_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), period VARCHAR(50),
    review_date TIMESTAMP WITHOUT TIME ZONE, chair_person VARCHAR(150),
    summary TEXT, status VARCHAR(50), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT m.id, m.code, m.title, m.period, m.review_date, m.chair_person, m.summary, m.status,
           COUNT(*) OVER() AS total_count
    FROM management_reviews m
    WHERE (p_search_term IS NULL OR m.title ILIKE '%' || p_search_term || '%' OR m.code ILIKE '%' || p_search_term || '%' OR m.chair_person ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR m.status ILIKE p_status)
    ORDER BY m.review_date DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_improvements_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status INT DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), title VARCHAR(250), current_state TEXT, future_state TEXT,
    source INT, expected_benefit TEXT, owner VARCHAR(150), status INT,
    related_review_id INT, related_finding_id INT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT i.id, i.code, i.title, i.current_state, i.future_state, i.source, i.expected_benefit,
           i.owner, i.status, i.related_review_id, i.related_finding_id, COUNT(*) OVER() AS total_count
    FROM improvements i
    WHERE (p_search_term IS NULL OR i.title ILIKE '%' || p_search_term || '%' OR i.code ILIKE '%' || p_search_term || '%' OR i.owner ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR i.status = p_status)
    ORDER BY i.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 9. ORGANIZATIONS, USERS & AUTHENTICATION
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_organizations_get_all()
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(200), industry VARCHAR(100), employees INT,
    primary_standard VARCHAR(100), status VARCHAR(50), compliance_percentage DOUBLE PRECISION,
    contact_email VARCHAR(150), created_at TIMESTAMP WITHOUT TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT o.id, o.code, o.name, o.industry, o.employees, o.primary_standard, o.status,
           o.compliance_percentage, o.contact_email, o.created_at
    FROM organizations o ORDER BY o.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_organizations_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_status VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, code VARCHAR(50), name VARCHAR(200), industry VARCHAR(100), employees INT,
    primary_standard VARCHAR(100), status VARCHAR(50), compliance_percentage DOUBLE PRECISION,
    contact_email VARCHAR(150), created_at TIMESTAMP WITHOUT TIME ZONE, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT o.id, o.code, o.name, o.industry, o.employees, o.primary_standard, o.status,
           o.compliance_percentage, o.contact_email, o.created_at,
           COUNT(*) OVER() AS total_count
    FROM organizations o
    WHERE (p_search_term IS NULL OR o.name ILIKE '%' || p_search_term || '%' OR o.code ILIKE '%' || p_search_term || '%' OR o.industry ILIKE '%' || p_search_term || '%')
      AND (p_status IS NULL OR o.status ILIKE p_status)
    ORDER BY o.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_users_get_all()
RETURNS TABLE (
    id INT, organization_id INT, name VARCHAR(150), email VARCHAR(150), password_hash VARCHAR(255),
    system_role INT, role VARCHAR(100), department VARCHAR(100), location VARCHAR(100),
    avatar_url VARCHAR(500), phone VARCHAR(50), bio TEXT, reset_token VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.organization_id, u.name, u.email, u.password_hash, u.system_role,
           u.role, u.department, u.location, u.avatar_url, u.phone, u.bio, u.reset_token
    FROM users u ORDER BY u.id ASC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_users_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL,
    p_department VARCHAR(100) DEFAULT NULL, p_organization_id INT DEFAULT NULL
)
RETURNS TABLE (
    id INT, organization_id INT, name VARCHAR(150), email VARCHAR(150), password_hash VARCHAR(255),
    system_role INT, role VARCHAR(100), department VARCHAR(100), location VARCHAR(100),
    avatar_url VARCHAR(500), phone VARCHAR(50), bio TEXT, reset_token VARCHAR(50), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.organization_id, u.name, u.email, u.password_hash, u.system_role,
           u.role, u.department, u.location, u.avatar_url, u.phone, u.bio, u.reset_token,
           COUNT(*) OVER() AS total_count
    FROM users u
    WHERE (p_search_term IS NULL OR u.name ILIKE '%' || p_search_term || '%' OR u.email ILIKE '%' || p_search_term || '%' OR u.department ILIKE '%' || p_search_term || '%' OR u.role ILIKE '%' || p_search_term || '%')
      AND (p_department IS NULL OR u.department ILIKE p_department)
      AND (p_organization_id IS NULL OR u.organization_id = p_organization_id)
    ORDER BY u.id ASC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_user_validate_login(p_email VARCHAR(150), p_password_hash VARCHAR(255))
RETURNS TABLE (
    id INT, organization_id INT, name VARCHAR(150), email VARCHAR(150),
    system_role INT, role VARCHAR(100), department VARCHAR(100), location VARCHAR(100),
    avatar_url VARCHAR(500), phone VARCHAR(50), bio TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.organization_id, u.name, u.email, u.system_role, u.role, u.department,
           u.location, u.avatar_url, u.phone, u.bio
    FROM users u
    WHERE LOWER(u.email) = LOWER(p_email) AND u.password_hash = p_password_hash;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 10. NOTIFICATIONS & AUDIT LOGS
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_notifications_get_all()
RETURNS TABLE (
    id INT, title VARCHAR(250), message TEXT, category VARCHAR(50),
    created_at TIMESTAMP WITHOUT TIME ZONE, is_read BOOLEAN, link_url VARCHAR(500)
) AS $$
BEGIN
    RETURN QUERY
    SELECT n.id, n.title, n.message, n.category, n.created_at, n.is_read, n.link_url
    FROM notifications n ORDER BY n.created_at DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_notifications_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_category VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    id INT, title VARCHAR(250), message TEXT, category VARCHAR(50),
    created_at TIMESTAMP WITHOUT TIME ZONE, is_read BOOLEAN, link_url VARCHAR(500), total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT n.id, n.title, n.message, n.category, n.created_at, n.is_read, n.link_url,
           COUNT(*) OVER() AS total_count
    FROM notifications n
    WHERE (p_search_term IS NULL OR n.title ILIKE '%' || p_search_term || '%' OR n.message ILIKE '%' || p_search_term || '%')
      AND (p_category IS NULL OR n.category ILIKE p_category)
    ORDER BY n.created_at DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_audit_logs_get_paged(
    p_page_number INT, p_page_size INT, p_search_term VARCHAR(200) DEFAULT NULL, p_module VARCHAR(100) DEFAULT NULL
)
RETURNS TABLE (
    id INT, "timestamp" TIMESTAMP WITHOUT TIME ZONE, "user" VARCHAR(150),
    action VARCHAR(100), module VARCHAR(100), entity_id VARCHAR(50), details TEXT, total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a."timestamp", a."user", a.action, a.module, a.entity_id, a.details,
           COUNT(*) OVER() AS total_count
    FROM audit_logs a
    WHERE (p_search_term IS NULL OR a."user" ILIKE '%' || p_search_term || '%' OR a.action ILIKE '%' || p_search_term || '%' OR a.details ILIKE '%' || p_search_term || '%')
      AND (p_module IS NULL OR a.module ILIKE p_module)
    ORDER BY a."timestamp" DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 11. MATRIX AGGREGATIONS & WORKFLOW PROCEDURES
-- ============================================================================

CREATE OR REPLACE FUNCTION sp_get_risk_heatmap_matrix()
RETURNS TABLE (
    likelihood INT, impact INT, risk_count BIGINT, risk_ids TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT r.likelihood, r.impact, COUNT(r.id) AS risk_count, STRING_AGG(r.code, ', ') AS risk_ids
    FROM risks r
    GROUP BY r.likelihood, r.impact
    ORDER BY r.likelihood DESC, r.impact DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE sp_process_archive(
    p_process_id INT,
    INOUT p_success BOOLEAN DEFAULT FALSE
) AS $$
BEGIN
    UPDATE processes
    SET status = 'Archived', updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata')
    WHERE id = p_process_id;

    IF FOUND THEN
        p_success := TRUE;
    ELSE
        p_success := FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;
