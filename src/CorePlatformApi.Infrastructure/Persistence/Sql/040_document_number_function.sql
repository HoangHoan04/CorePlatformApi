-- Sinh tự động từ docs/base_erp_core.sql (đã làm phẳng về schema public) — các phần EF Core không biểu diễn được.
CREATE OR REPLACE FUNCTION next_document_number(
    p_tenant_id uuid,
    p_document_type varchar,
    p_company_id uuid DEFAULT NULL,
    p_branch_id uuid DEFAULT NULL,
    p_date date DEFAULT current_date
) RETURNS text
LANGUAGE plpgsql AS $$
DECLARE
    v_rule        numbering_rules%ROWTYPE;
    v_period      text;
    v_scope       text;
    v_value       bigint;
    v_pattern     text;
    v_result      text;
    v_branch_code text;
    v_company_code text;
    v_token       text;
BEGIN
    -- Chọn quy tắc cụ thể nhất: chi nhánh > công ty > toàn tenant
    SELECT * INTO v_rule
    FROM numbering_rules r
    WHERE r.tenant_id = p_tenant_id
      AND r.document_type_code = p_document_type
      AND r.is_active AND NOT r.is_deleted
      AND (r.branch_id IS NULL OR r.branch_id = p_branch_id)
      AND (r.company_id IS NULL OR r.company_id = p_company_id)
    ORDER BY (r.branch_id IS NOT NULL) DESC, (r.company_id IS NOT NULL) DESC
    LIMIT 1;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Chưa cấu hình quy tắc sinh mã cho loại chứng từ %', p_document_type
            USING ERRCODE = 'P0002';
    END IF;

    v_period := CASE v_rule.reset_policy
                    WHEN 'YEARLY'  THEN to_char(p_date, 'YYYY')
                    WHEN 'MONTHLY' THEN to_char(p_date, 'YYYYMM')
                    WHEN 'DAILY'   THEN to_char(p_date, 'YYYYMMDD')
                    ELSE 'ALL' END;
    v_scope := CASE v_rule.counter_scope
                    WHEN 'COMPANY' THEN coalesce(p_company_id::text, '-')
                    WHEN 'BRANCH'  THEN coalesce(p_branch_id::text, '-')
                    ELSE '-' END;

    INSERT INTO numbering_counters AS c (rule_id, period_key, scope_key, current_value)
    VALUES (v_rule.id, v_period, v_scope, v_rule.start_value)
    ON CONFLICT (rule_id, period_key, scope_key)
    DO UPDATE SET current_value = c.current_value + v_rule.step, updated_at = now()
    RETURNING current_value INTO v_value;

    SELECT code INTO v_branch_code FROM branches WHERE id = p_branch_id;
    SELECT code INTO v_company_code FROM companies WHERE id = p_company_id;

    v_pattern := coalesce(v_rule.pattern,
                          coalesce(v_rule.prefix, '') || '{' || repeat('0', v_rule.padding_length) || '}' || coalesce(v_rule.suffix, ''));

    v_result := v_pattern;
    v_result := replace(v_result, '{YYYY}', to_char(p_date, 'YYYY'));
    v_result := replace(v_result, '{YYMM}', to_char(p_date, 'YYMM'));
    v_result := replace(v_result, '{YY}',   to_char(p_date, 'YY'));
    v_result := replace(v_result, '{MM}',   to_char(p_date, 'MM'));
    v_result := replace(v_result, '{DD}',   to_char(p_date, 'DD'));
    v_result := replace(v_result, '{BRANCH}',  coalesce(v_branch_code, ''));
    v_result := replace(v_result, '{COMPANY}', coalesce(v_company_code, ''));

    -- Token số tự tăng {0000} hoặc {00001}: độ dài phần số = số ký tự trong ngoặc
    v_token := substring(v_result FROM '\{[0-9]+\}');
    IF v_token IS NOT NULL THEN
        v_result := replace(v_result, v_token,
                            CASE WHEN length(v_value::text) >= length(v_token) - 2 THEN v_value::text
                                 ELSE lpad(v_value::text, length(v_token) - 2, '0') END);
    END IF;

    RETURN v_result;
END $$;
COMMENT ON FUNCTION next_document_number(uuid, varchar, uuid, uuid, date) IS 'Cấp số chứng từ tiếp theo theo quy tắc numbering_rules; an toàn khi nhiều request đồng thời';
