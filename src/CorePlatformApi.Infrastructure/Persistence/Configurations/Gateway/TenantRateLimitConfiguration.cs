// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho <c>gateway.tenant_rate_limits</c>.
/// </summary>
public sealed class TenantRateLimitConfiguration : IEntityTypeConfiguration<TenantRateLimit>
{
    public void Configure(EntityTypeBuilder<TenantRateLimit> builder)
    {
        builder.ToTable("tenant_rate_limits", "gateway", tb =>
        {
            tb.HasComment("Ghi đè hạn mức rate limit cho tenant cụ thể (gói cao cấp được nhiều request hơn)");
        });

        builder.HasKey(x => x.Id).HasName("tenant_rate_limits_pkey");

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
        builder.Property(x => x.PolicyId)
            .HasColumnName("policy_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Chính sách gốc");
        builder.Property(x => x.PermitLimit)
            .HasColumnName("permit_limit")
            .HasColumnType("int")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Hạn mức riêng");
        builder.Property(x => x.WindowSeconds)
            .HasColumnName("window_seconds")
            .HasColumnType("int")
            .HasColumnOrder(4)
            .HasComment("Cửa sổ riêng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.PolicyId }, "uq_tenant_rate_limits").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("tenant_rate_limits_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Policy)
            .WithMany(p => p.TenantRateLimits)
            .HasForeignKey(x => x.PolicyId)
            .HasConstraintName("tenant_rate_limits_policy_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
