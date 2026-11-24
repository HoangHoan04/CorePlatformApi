// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>conversations</c>.
/// </summary>
public sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("conversations", tb =>
        {
            tb.HasComment("Hội thoại chatbot nội bộ");
        });

        builder.HasKey(x => x.Id).HasName("conversations_pkey");

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
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Người hỏi");
        builder.Property(x => x.KnowledgeBaseId)
            .HasColumnName("knowledge_base_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Kho tri thức (NULL = chat tự do)");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(4)
            .HasComment("Phân hệ mở chatbot");
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .HasComment("Tiêu đề (tự sinh từ câu đầu)");
        builder.Property(x => x.IsArchived)
            .HasColumnName("is_archived")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Lưu trữ");
        builder.Property(x => x.LastMessageAt)
            .HasColumnName("last_message_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Tin nhắn cuối");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Tạo lúc");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.UserId, x.LastMessageAt }, "ix_ai_conversations_user")
            .IsDescending(false, true);

        builder.HasOne(x => x.User)
            .WithMany(p => p.AiConversations)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("conversations_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.KnowledgeBase)
            .WithMany()
            .HasForeignKey(x => x.KnowledgeBaseId)
            .HasConstraintName("conversations_knowledge_base_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
