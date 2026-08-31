-- ============================================================================
-- ISOFlow Platform - Database Functions (Scalar & Table-Valued)
-- Compatible with: PostgreSQL 12+ / Azure PostgreSQL / Supabase
-- Schema Version: 2.1.0 (Integer Primary Keys + IST Timezone)
-- ============================================================================

-- 1. Calculate Risk Score (Scalar)
CREATE OR REPLACE FUNCTION fn_calculate_risk_score(
    p_likelihood INT,
    p_impact INT
)
RETURNS INT AS $$
BEGIN
    RETURN COALESCE(p_likelihood, 1) * COALESCE(p_impact, 1);
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- 2. Determine Risk Level Classification (Scalar)
CREATE OR REPLACE FUNCTION fn_determine_risk_level(
    p_score INT
)
RETURNS VARCHAR(20) AS $$
BEGIN
    IF p_score >= 15 THEN
        RETURN 'Critical';
    ELSIF p_score >= 10 THEN
        RETURN 'High';
    ELSIF p_score >= 5 THEN
        RETURN 'Medium';
    ELSE
        RETURN 'Low';
    END IF;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- 3. Get Overdue Tasks Count for an Owner (Scalar)
CREATE OR REPLACE FUNCTION fn_get_overdue_tasks_count(
    p_owner VARCHAR(150)
)
RETURNS INT AS $$
DECLARE
    v_count INT;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM task_items
    WHERE (p_owner IS NULL OR owner = p_owner)
      AND status != 3 -- 3 = Completed
      AND due_date < (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata');
      
    RETURN v_count;
END;
$$ LANGUAGE plpgsql STABLE;

-- 4. Get Standard Compliance Summary (Table-Valued)
CREATE OR REPLACE FUNCTION fn_get_standard_compliance_summary(
    p_standard_id INT
)
RETURNS TABLE (
    standard_id INT,
    standard_code VARCHAR(50),
    standard_name VARCHAR(200),
    total_requirements BIGINT,
    average_compliance DOUBLE PRECISION,
    implemented_controls_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        s.id AS standard_id,
        s.code AS standard_code,
        s.name AS standard_name,
        COUNT(DISTINCT r.id) AS total_requirements,
        COALESCE(AVG(r.compliance_percentage), 0.0) AS average_compliance,
        COUNT(DISTINCT CASE WHEN c.status IN (2, 3) THEN c.id END) AS implemented_controls_count
    FROM standards s
    LEFT JOIN requirements r ON s.id = r.standard_id
    LEFT JOIN controls c ON s.id = c.standard_id
    WHERE (p_standard_id IS NULL OR s.id = p_standard_id)
    GROUP BY s.id, s.code, s.name;
END;
$$ LANGUAGE plpgsql STABLE;
