// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.models</c>.
/// </summary>
public sealed class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("models", "ai", tb =>
        {
            tb.HasComment("Model AI và bảng giá để ước tính chi phí");
        });

        builder.HasKey(x => x.Id).HasName("models_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.ProviderId)
            .HasColumnName("provider_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Nhà cung cấp");
        builder.Property(x => x.ModelCode)
            .HasColumnName("model_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã model gửi lên API");
        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên hiển thị");
        builder.Property(x => x.Capabilities)
            .HasColumnName("capabilities")
            .HasColumnType("text[]")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'{CHAT}'")
            .ValueGeneratedNever()
            .HasComment("Năng lực: CHAT, VISION, TOOLS, JSON_MODE, EMBEDDING, RERANK, AUDIO, IMAGE_GEN");
        builder.Property(x => x.ContextWindow)
            .HasColumnName("context_window")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .HasComment("Số token ngữ cảnh tối đa");
        builder.Property(x => x.MaxOutputTokens)
            .HasColumnName("max_output_tokens")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .HasComment("Số token đầu ra tối đa");
        builder.Property(x => x.EmbeddingDimension)
            .HasColumnName("embedding_dimension")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Số chiều vector (model embedding)");
        builder.Property(x => x.InputPricePer1m)
            .HasColumnName("input_price_per_1m")
            .HasColumnType("numeric(12,6)")
            .HasColumnOrder(8)
            .HasComment("Giá input / 1 triệu token (USD)");
        builder.Property(x => x.OutputPricePer1m)
            .HasColumnName("output_price_per_1m")
            .HasColumnType("numeric(12,6)")
            .HasColumnOrder(9)
            .HasComment("Giá output / 1 triệu token (USD)");
        builder.Property(x => x.CachedInputPricePer1m)
            .HasColumnName("cached_input_price_per_1m")
            .HasColumnType("numeric(12,6)")
            .HasColumnOrder(10)
            .HasComment("Giá input cache / 1 triệu token");
        builder.Property(x => x.PriceCurrency)
            .HasColumnName("price_currency")
            .HasColumnType("char(3)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'USD'")
            .ValueGeneratedNever()
            .HasComment("Tiền tệ bảng giá");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
        builder.Property(x => x.DeprecatedAt)
            .HasColumnName("deprecated_at")
            .HasColumnType("date")
            .HasColumnOrder(13)
            .HasComment("Ngày provider ngừng hỗ trợ");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.ProviderId, x.ModelCode }, "uq_ai_models").IsUnique();

        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .HasConstraintName("models_provider_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
