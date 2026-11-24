-- Sinh tự động từ docs/base_erp_core.sql (đã làm phẳng về schema public) — các phần EF Core không biểu diễn được.
-- pgvector là extension ngoài (không có sẵn trong PostgreSQL). Script idempotent:
--   * Nếu server đã cài pgvector  -> tạo extension, cột kb_chunks.embedding và index HNSW.
--   * Nếu chưa cài                -> bỏ qua kèm NOTICE; ứng dụng chạy lại script này mỗi lần khởi động.
DO $pgv$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_available_extensions WHERE name = 'vector') THEN
        RAISE NOTICE 'pgvector chưa được cài trên server — bỏ qua cột kb_chunks.embedding';
        RETURN;
    END IF;

    CREATE EXTENSION IF NOT EXISTS vector;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'kb_chunks' AND column_name = 'embedding'
    ) THEN
        EXECUTE 'ALTER TABLE kb_chunks ADD COLUMN embedding vector(1536)';
        EXECUTE $c$COMMENT ON COLUMN kb_chunks.embedding IS 'Vector embedding; đổi số chiều theo model (VD: 768 Gemini, 3072 text-embedding-3-large dùng halfvec)'$c$;
    END IF;

    EXECUTE 'CREATE INDEX IF NOT EXISTS ix_ai_kb_chunks_embedding ON kb_chunks USING hnsw (embedding vector_cosine_ops)';
END $pgv$;
