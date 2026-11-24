// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>quotas</c>.
/// </summary>
public sealed class AiQuotaConfiguration : IEntityTypeConfiguration<AiQuota>
{
    public void Configure(EntityTypeBuilder<AiQuota> builder)
    {
        builder.ToTable("quotas", tb =>
        {
            tb.HasComment("Hạn mức sử dụng AI theo tenant / phân hệ / user theo ngày hoặc tháng");
            tb.HasCheckConstraint("quotas_period_check", "period IN ('DAILY','MONTHLY')");
            tb.HasCheckConstraint("quotas_alert_threshold_pct_check", "alert_threshold_pct BETWEEN 1 AND 100");
            tb.HasCheckConstraint("quotas_action_on_exceed_check", "action_on_exceed IN ('BLOCK','ALERT_ONLY','DOWNGRADE')");
        });

        builder.HasKey(x => x.Id).HasName("quotas_pkey");

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
            .HasComment("Phân hệ (NULL = toàn tenant)");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("User (NULL = không giới hạn theo user)");
        builder.Property(x => x.Period)
            .HasColumnName("period")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Chu kỳ");
        builder.Property(x => x.MaxRequests)
            .HasColumnName("max_requests")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .HasComment("Số request tối đa");
        builder.Property(x => x.MaxTokens)
            .HasColumnName("max_tokens")
            .HasColumnType("bigint")
            .HasColumnOrder(6)
            .HasComment("Tổng token tối đa");
        builder.Property(x => x.MaxCostUsd)
            .HasColumnName("max_cost_usd")
            .HasColumnType("numeric(12,4)")
            .HasColumnOrder(7)
            .HasComment("Chi phí tối đa (USD)");
        builder.Property(x => x.AlertThresholdPct)
            .HasColumnName("alert_threshold_pct")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("80")
            .ValueGeneratedNever()
            .HasComment("Ngưỡng % gửi cảnh báo");
        builder.Property(x => x.ActionOnExceed)
            .HasColumnName("action_on_exceed")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'BLOCK'")
            .ValueGeneratedNever()
            .HasComment("Khi vượt: chặn, chỉ cảnh báo, chuyển model rẻ hơn");
        builder.Property(x => x.DowngradeAliasId)
            .HasColumnName("downgrade_alias_id")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Alias rẻ hơn khi DOWNGRADE");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang áp dụng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
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

        builder.HasIndex(x => new { x.TenantId, x.ModuleId, x.UserId, x.Period }, "uq_ai_quotas")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("quotas_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("quotas_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("quotas_user_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DowngradeAlias)
            .WithMany()
            .HasForeignKey(x => x.DowngradeAliasId)
            .HasConstraintName("quotas_downgrade_alias_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
