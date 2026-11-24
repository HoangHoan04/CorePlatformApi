// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>messages</c>.
/// </summary>
public sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("messages", tb =>
        {
            tb.HasComment("Tin nhắn trong hội thoại, kèm trích dẫn chunk nguồn và đánh giá của người dùng");
            tb.HasCheckConstraint("messages_role_check", "role IN ('USER','ASSISTANT','SYSTEM','TOOL')");
            tb.HasCheckConstraint("messages_feedback_check", "feedback IN (-1, 1)");
        });

        builder.HasKey(x => x.Id).HasName("messages_pkey");

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
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.ConversationId)
            .HasColumnName("conversation_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Hội thoại");
        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Vai trò");
        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Nội dung");
        builder.Property(x => x.Citations)
            .HasColumnName("citations")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .HasComment("Nguồn trích dẫn [{\"chunk_id\":\"...\",\"document_id\":\"...\",\"score\":0.82}]");
        builder.Property(x => x.UsageLogId)
            .HasColumnName("usage_log_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Liên kết ai.usage_logs");
        builder.Property(x => x.InputTokens)
            .HasColumnName("input_tokens")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Token vào");
        builder.Property(x => x.OutputTokens)
            .HasColumnName("output_tokens")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .HasComment("Token ra");
        builder.Property(x => x.Feedback)
            .HasColumnName("feedback")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .HasComment("Đánh giá: 1 hữu ích, -1 không hữu ích");
        builder.Property(x => x.FeedbackNote)
            .HasColumnName("feedback_note")
            .HasColumnType("text")
            .HasColumnOrder(10)
            .HasComment("Góp ý");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt }, "ix_ai_messages_conversation");

        builder.HasOne(x => x.Conversation)
            .WithMany(p => p.AiMessages)
            .HasForeignKey(x => x.ConversationId)
            .HasConstraintName("messages_conversation_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
