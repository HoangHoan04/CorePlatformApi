-- Sinh tự động từ docs/base_erp_core.sql (đã làm phẳng về schema public) — các phần EF Core không biểu diễn được.
DO $$
DECLARE r record;
BEGIN
    FOR r IN
        SELECT c.table_schema, c.table_name
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema = c.table_schema AND t.table_name = c.table_name AND t.table_type = 'BASE TABLE'
        WHERE c.column_name = 'updated_at'
          AND c.table_schema = 'public'
    LOOP
        EXECUTE format('CREATE OR REPLACE TRIGGER trg_%s_updated_at BEFORE UPDATE ON %I.%I FOR EACH ROW EXECUTE FUNCTION set_updated_at()',
                       r.table_name, r.table_schema, r.table_name);
    END LOOP;
END $$;
