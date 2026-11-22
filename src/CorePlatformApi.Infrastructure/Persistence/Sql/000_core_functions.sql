-- Sinh tự động từ docs/base_erp_core.sql — các phần EF Core không biểu diễn được.
CREATE SCHEMA IF NOT EXISTS core;
CREATE SCHEMA IF NOT EXISTS org;
CREATE SCHEMA IF NOT EXISTS iam;
CREATE SCHEMA IF NOT EXISTS gateway;
CREATE SCHEMA IF NOT EXISTS mdm;
CREATE SCHEMA IF NOT EXISTS storage;
CREATE SCHEMA IF NOT EXISTS notify;
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS monitor;
CREATE SCHEMA IF NOT EXISTS workflow;
CREATE SCHEMA IF NOT EXISTS integration;
CREATE SCHEMA IF NOT EXISTS ai;
CREATE SCHEMA IF NOT EXISTS config;
CREATE SCHEMA IF NOT EXISTS job;

COMMENT ON SCHEMA core        IS 'Hàm tiện ích dùng chung (trigger, partition)';
COMMENT ON SCHEMA org         IS 'Cấu trúc tổ chức đa tenant: Tập đoàn → Công ty con → Chi nhánh → Phòng ban';
COMMENT ON SCHEMA iam         IS 'Identity & Access Management: tài khoản, SSO, phiên, token, RBAC, data scope';
COMMENT ON SCHEMA gateway     IS 'Cấu hình API Gateway: route, upstream, rate limit, CORS, security header, mã lỗi';
COMMENT ON SCHEMA mdm         IS 'Master Data Management: danh mục dùng chung toàn hệ sinh thái';
COMMENT ON SCHEMA storage     IS 'File & Document Storage tập trung trên Object Storage';
COMMENT ON SCHEMA notify      IS 'Notification Center đa kênh: in-app, push, email';
COMMENT ON SCHEMA audit       IS 'Audit log: đăng nhập, bảo mật, thay đổi dữ liệu, request API';
COMMENT ON SCHEMA monitor     IS 'Giám sát hệ thống: danh mục service, health check';
COMMENT ON SCHEMA workflow    IS 'Dynamic Workflow & Approval Engine';
COMMENT ON SCHEMA integration IS 'Integration Hub: event bus, webhook, API key, kết nối bên thứ 3';
COMMENT ON SCHEMA ai          IS 'AI Gateway: provider, model, prompt, guardrail, quota, RAG';
COMMENT ON SCHEMA config      IS 'Cấu hình động, feature flag, i18n';
COMMENT ON SCHEMA job         IS 'Background job scheduler (metadata quản trị, Hangfire/Quartz tự quản lý storage riêng)';

CREATE OR REPLACE FUNCTION core.set_updated_at() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at := now();
    RETURN NEW;
END $$;
COMMENT ON FUNCTION core.set_updated_at() IS 'Trigger BEFORE UPDATE: tự gán updated_at = now()';

-- Tạo partition theo tháng cho bảng log: SELECT core.create_monthly_partitions('audit.login_logs', '2026-11-01', 12);
CREATE OR REPLACE FUNCTION core.create_monthly_partitions(p_parent regclass, p_from date, p_months int)
RETURNS void LANGUAGE plpgsql AS $$
DECLARE
    v_start date := date_trunc('month', p_from)::date;
    v_end   date;
    v_name  text;
    v_schema text;
    v_table  text;
BEGIN
    SELECT n.nspname, c.relname INTO v_schema, v_table
    FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
    WHERE c.oid = p_parent;

    FOR i IN 0 .. p_months - 1 LOOP
        v_end  := (v_start + interval '1 month')::date;
        v_name := format('%s_y%sm%s', v_table, to_char(v_start, 'YYYY'), to_char(v_start, 'MM'));
        EXECUTE format('CREATE TABLE IF NOT EXISTS %I.%I PARTITION OF %s FOR VALUES FROM (%L) TO (%L)',
                       v_schema, v_name, p_parent, v_start, v_end);
        v_start := v_end;
    END LOOP;
END $$;
COMMENT ON FUNCTION core.create_monthly_partitions(regclass, date, int) IS 'Tạo N partition theo tháng cho bảng partition RANGE; gọi định kỳ bằng job';
