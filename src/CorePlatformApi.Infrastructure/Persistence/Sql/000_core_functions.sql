-- Sinh tự động từ docs/base_erp_core.sql (đã làm phẳng về schema public) — các phần EF Core không biểu diễn được.
CREATE OR REPLACE FUNCTION set_updated_at() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at := now();
    RETURN NEW;
END $$;
COMMENT ON FUNCTION set_updated_at() IS 'Trigger BEFORE UPDATE: tự gán updated_at = now()';
