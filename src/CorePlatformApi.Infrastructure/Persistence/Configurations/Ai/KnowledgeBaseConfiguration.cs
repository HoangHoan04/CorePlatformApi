// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>knowledge_bases</c>.
/// </summary>
public sealed class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBase>
{
    public void Configure(EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.ToTable("knowledge_bases", tb =>
        {
            tb.HasComment("Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ");
            tb.HasCheckConstraint("knowledge_bases_visibility_check", "visibility IN ('ALL_USERS','RESTRICTED')");
            tb.HasCheckConstraint("knowledge_bases_status_check", "status IN ('ACTIVE','INDEXING','DISABLED')");
        });

        builder.HasKey(x => x.Id).HasName("knowledge_bases_pkey");

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
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Phân hệ liên quan (NULL = dùng chung)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã kho");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên kho tri thức");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.EmbeddingModelId)
            .HasColumnName("embedding_model_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Model embedding (đổi model phải re-index)");
        builder.Property(x => x.EmbeddingDimension)
            .HasColumnName("embedding_dimension")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("1536")
            .ValueGeneratedNever()
            .HasComment("Số chiều vector");
        builder.Property(x => x.ChunkSize)
            .HasColumnName("chunk_size")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("800")
            .ValueGeneratedNever()
            .HasComment("Kích thước chunk (token)");
        builder.Property(x => x.ChunkOverlap)
            .HasColumnName("chunk_overlap")
            .HasColumnType("int")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("100")
            .ValueGeneratedNever()
            .HasComment("Phần chồng lấn giữa các chunk");
        builder.Property(x => x.RetrievalTopK)
            .HasColumnName("retrieval_top_k")
            .HasColumnType("smallint")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("5")
            .ValueGeneratedNever()
            .HasComment("Số chunk lấy mỗi truy vấn");
        builder.Property(x => x.MinScore)
            .HasColumnName("min_score")
            .HasColumnType("numeric(4,3)")
            .HasColumnOrder(11)
            .HasDefaultValueSql("0.3")
            .ValueGeneratedNever()
            .HasComment("Ngưỡng tương đồng tối thiểu");
        builder.Property(x => x.AnswerPromptId)
            .HasColumnName("answer_prompt_id")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Prompt trả lời dùng cho kho này");
        builder.Property(x => x.Visibility)
            .HasColumnName("visibility")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("'RESTRICTED'")
            .ValueGeneratedNever()
            .HasComment("Ai được hỏi đáp");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người xóa mềm (iam.users.id)");


        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("knowledge_bases_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("knowledge_bases_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.EmbeddingModel)
            .WithMany()
            .HasForeignKey(x => x.EmbeddingModelId)
            .HasConstraintName("knowledge_bases_embedding_model_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.AnswerPrompt)
            .WithMany()
            .HasForeignKey(x => x.AnswerPromptId)
            .HasConstraintName("knowledge_bases_answer_prompt_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
