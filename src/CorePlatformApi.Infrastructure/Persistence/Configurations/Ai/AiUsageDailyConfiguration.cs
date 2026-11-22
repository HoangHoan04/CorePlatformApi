// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.usage_daily</c>.
/// </summary>
public sealed class AiUsageDailyConfiguration : IEntityTypeConfiguration<AiUsageDaily>
{
    public void Configure(EntityTypeBuilder<AiUsageDaily> builder)
    {
        builder.ToTable("usage_daily", "ai", tb =>
        {
            tb.HasComment("Tổng hợp usage theo ngày (job cộng dồn từ usage_logs) — dùng kiểm tra quota nhanh và báo cáo chi phí");
        });

        builder.HasKey(x => new { x.UsageDate, x.TenantId, x.ModuleCode, x.ModelId }).HasName("usage_daily_pkey");

        builder.Property(x => x.UsageDate)
            .HasColumnName("usage_date")
            .HasColumnType("date")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Ngày");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasDefaultValueSql("'-'")
            .ValueGeneratedNever()
            .HasComment("Phân hệ ('-' nếu không xác định)");
        builder.Property(x => x.ModelId)
            .HasColumnName("model_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Model");
        builder.Property(x => x.RequestCount)
            .HasColumnName("request_count")
            .HasColumnType("int")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số request");
        builder.Property(x => x.ErrorCount)
            .HasColumnName("error_count")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số request lỗi");
        builder.Property(x => x.InputTokens)
            .HasColumnName("input_tokens")
            .HasColumnType("bigint")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng token vào");
        builder.Property(x => x.OutputTokens)
            .HasColumnName("output_tokens")
            .HasColumnType("bigint")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng token ra");
        builder.Property(x => x.CostUsd)
            .HasColumnName("cost_usd")
            .HasColumnType("numeric(14,6)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng chi phí");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Cập nhật lúc");

    }
}
