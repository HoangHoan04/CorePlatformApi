// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Pgvector;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>kb_chunks</c>.
/// </summary>
public sealed class KbChunkConfiguration : IEntityTypeConfiguration<KbChunk>
{
    public void Configure(EntityTypeBuilder<KbChunk> builder)
    {
        builder.ToTable("kb_chunks", tb =>
        {
            tb.HasComment("Chunk văn bản + vector embedding (pgvector) + full-text để hybrid search");
        });

        builder.HasKey(x => x.Id).HasName("kb_chunks_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tài liệu");
        builder.Property(x => x.KnowledgeBaseId)
            .HasColumnName("knowledge_base_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Kho (denormalize để lọc nhanh)");
        builder.Property(x => x.ChunkIndex)
            .HasColumnName("chunk_index")
            .HasColumnType("int")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Thứ tự chunk trong tài liệu");
        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Nội dung chunk");
        builder.Property(x => x.HeadingPath)
            .HasColumnName("heading_path")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Đường dẫn tiêu đề (Chương 2 > Điều 5) để trích dẫn");
        builder.Property(x => x.PageNumber)
            .HasColumnName("page_number")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Số trang (PDF)");
        builder.Property(x => x.TokenCount)
            .HasColumnName("token_count")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .HasComment("Số token");
        builder.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Metadata lọc (department, tags...)");
        builder.Property(x => x.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(1536)")
            .HasColumnOrder(10)
            .HasConversion(
                v => v == null ? null : new Vector(v),
                v => v == null ? null : v.ToArray(),
                new ValueComparer<float[]?>(
                    (a, b) => a == null ? b == null : b != null && a.SequenceEqual(b),
                    v => v == null ? 0 : v.Aggregate(0, (h, x) => HashCode.Combine(h, x)),
                    v => v == null ? null : v.ToArray()))
            .HasComment("Vector embedding; đổi số chiều theo model (VD: 768 Gemini, 3072 text-embedding-3-large dùng halfvec)");
        builder.Property<NpgsqlTsVector>("ContentTsv")
            .HasColumnName("content_tsv")
            .HasColumnType("tsvector")
            .HasColumnOrder(11)
            .HasComputedColumnSql("to_tsvector('simple', content)", stored: true)
            .HasComment("Chỉ mục full-text (cấu hình 'simple' phù hợp tiếng Việt)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Tạo lúc");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.DocumentId, x.ChunkIndex }, "uq_ai_kb_chunks").IsUnique();
        builder.HasIndex(new[] { "ContentTsv" }, "ix_ai_kb_chunks_tsv")
            .HasMethod("gin");
        builder.HasIndex(x => x.KnowledgeBaseId, "ix_ai_kb_chunks_kb");

        builder.HasOne(x => x.Document)
            .WithMany(p => p.KbChunks)
            .HasForeignKey(x => x.DocumentId)
            .HasConstraintName("kb_chunks_document_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.KnowledgeBase)
            .WithMany(p => p.KbChunks)
            .HasForeignKey(x => x.KnowledgeBaseId)
            .HasConstraintName("kb_chunks_knowledge_base_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
