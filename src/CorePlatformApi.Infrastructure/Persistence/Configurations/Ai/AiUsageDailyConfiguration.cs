// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>usage_daily</c>.
/// </summary>
public sealed class AiUsageDailyConfiguration : IEntityTypeConfiguration<AiUsageDaily>
{
    public void Configure(EntityTypeBuilder<AiUsageDaily> builder)
    {
        builder.ToTable("usage_daily", tb =>
        {
            tb.HasComment("Tổng hợp usage theo ngày (job cộng dồn từ usage_logs) — dùng kiểm tra quota nhanh và báo cáo chi phí");
        });

        builder.HasKey(x => x.Id).HasName("usage_daily_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (UUID v7)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.UsageDate)
            .HasColumnName("usage_date")
            .HasColumnType("date")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Ngày");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'-'")
            .ValueGeneratedNever()
            .HasComment("Phân hệ ('-' nếu không xác định)");
        builder.Property(x => x.ModelId)
            .HasColumnName("model_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Model");
        builder.Property(x => x.RequestCount)
            .HasColumnName("request_count")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số request");
        builder.Property(x => x.ErrorCount)
            .HasColumnName("error_count")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số request lỗi");
        builder.Property(x => x.InputTokens)
            .HasColumnName("input_tokens")
            .HasColumnType("bigint")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng token vào");
        builder.Property(x => x.OutputTokens)
            .HasColumnName("output_tokens")
            .HasColumnType("bigint")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng token ra");
        builder.Property(x => x.CostUsd)
            .HasColumnName("cost_usd")
            .HasColumnType("numeric(14,6)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng chi phí");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Cập nhật lúc");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.UsageDate, x.TenantId, x.ModuleCode, x.ModelId }, "usage_daily_usage_date_tenant_id_module_code_model_id_key").IsUnique();
    }
}
