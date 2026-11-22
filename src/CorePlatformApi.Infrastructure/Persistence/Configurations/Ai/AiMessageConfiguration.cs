// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.messages</c>.
/// </summary>
public sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("messages", "ai", tb =>
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
        builder.Property(x => x.ConversationId)
            .HasColumnName("conversation_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Hội thoại");
        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Vai trò");
        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Nội dung");
        builder.Property(x => x.Citations)
            .HasColumnName("citations")
            .HasColumnType("jsonb")
            .HasColumnOrder(4)
            .HasComment("Nguồn trích dẫn [{\"chunk_id\":\"...\",\"document_id\":\"...\",\"score\":0.82}]");
        builder.Property(x => x.UsageLogId)
            .HasColumnName("usage_log_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Liên kết ai.usage_logs");
        builder.Property(x => x.InputTokens)
            .HasColumnName("input_tokens")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .HasComment("Token vào");
        builder.Property(x => x.OutputTokens)
            .HasColumnName("output_tokens")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Token ra");
        builder.Property(x => x.Feedback)
            .HasColumnName("feedback")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .HasComment("Đánh giá: 1 hữu ích, -1 không hữu ích");
        builder.Property(x => x.FeedbackNote)
            .HasColumnName("feedback_note")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("Góp ý");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt }, "ix_ai_messages_conversation");

        builder.HasOne(x => x.Conversation)
            .WithMany(p => p.AiMessages)
            .HasForeignKey(x => x.ConversationId)
            .HasConstraintName("messages_conversation_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
