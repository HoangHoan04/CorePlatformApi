// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.kb_documents</c>.
/// </summary>
public sealed class KbDocumentConfiguration : IEntityTypeConfiguration<KbDocument>
{
    public void Configure(EntityTypeBuilder<KbDocument> builder)
    {
        builder.ToTable("kb_documents", "ai", tb =>
        {
            tb.HasComment("Tài liệu nguồn trong kho tri thức");
            tb.HasCheckConstraint("kb_documents_source_type_check", "source_type IN ('FILE','URL','TEXT','ENTITY')");
            tb.HasCheckConstraint("kb_documents_status_check", "status IN ('PENDING','PROCESSING','INDEXED','FAILED','DISABLED')");
        });

        builder.HasKey(x => x.Id).HasName("kb_documents_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.KnowledgeBaseId)
            .HasColumnName("knowledge_base_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Kho tri thức");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tenant (denormalize)");
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tiêu đề");
        builder.Property(x => x.SourceType)
            .HasColumnName("source_type")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Nguồn: file, URL, văn bản nhập tay, bản ghi nghiệp vụ");
        builder.Property(x => x.FileId)
            .HasColumnName("file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("File gốc");
        builder.Property(x => x.SourceUrl)
            .HasColumnName("source_url")
            .HasColumnType("varchar(1000)")
            .HasColumnOrder(6)
            .HasComment("URL nguồn");
        builder.Property(x => x.ContentText)
            .HasColumnName("content_text")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Nội dung text (TEXT) hoặc text đã trích xuất");
        builder.Property(x => x.ContentHash)
            .HasColumnName("content_hash")
            .HasColumnType("char(64)")
            .HasColumnOrder(8)
            .HasComment("Hash nội dung để bỏ qua re-index khi không đổi");
        builder.Property(x => x.Language)
            .HasColumnName("language")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(9)
            .HasDefaultValueSql("'vi'")
            .ValueGeneratedNever()
            .HasComment("Ngôn ngữ");
        builder.Property(x => x.Tags)
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .HasColumnOrder(10)
            .HasComment("Thẻ phân loại");
        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("effective_from")
            .HasColumnType("date")
            .HasColumnOrder(11)
            .HasComment("Hiệu lực từ (VD: nội quy bản 2026)");
        builder.Property(x => x.EffectiveTo)
            .HasColumnName("effective_to")
            .HasColumnType("date")
            .HasColumnOrder(12)
            .HasComment("Hết hiệu lực → loại khỏi kết quả tìm kiếm");
        builder.Property(x => x.ChunkCount)
            .HasColumnName("chunk_count")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số chunk đã tạo");
        builder.Property(x => x.TokenCount)
            .HasColumnName("token_count")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .HasComment("Tổng token");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái index");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(16)
            .HasComment("Lỗi index");
        builder.Property(x => x.IndexedAt)
            .HasColumnName("indexed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Index xong lúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => x.KnowledgeBaseId, "ix_ai_kb_documents_kb")
            .HasFilter("NOT is_deleted");

        builder.HasOne(x => x.KnowledgeBase)
            .WithMany(p => p.KbDocuments)
            .HasForeignKey(x => x.KnowledgeBaseId)
            .HasConstraintName("kb_documents_knowledge_base_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("kb_documents_file_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
