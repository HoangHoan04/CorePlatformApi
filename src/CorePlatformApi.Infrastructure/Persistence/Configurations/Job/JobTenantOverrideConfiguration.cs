// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Job;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Job;

/// <summary>
/// Cấu hình EF Core cho bảng <c>tenant_overrides</c>.
/// </summary>
public sealed class JobTenantOverrideConfiguration : IEntityTypeConfiguration<JobTenantOverride>
{
    public void Configure(EntityTypeBuilder<JobTenantOverride> builder)
    {
        builder.ToTable("tenant_overrides", tb =>
        {
            tb.HasComment("Ghi đè lịch/tham số job theo tenant (VD: tenant A chốt công lúc 23:00, tenant B lúc 01:00)");
        });

        builder.HasKey(x => x.Id).HasName("tenant_overrides_pkey");

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
        builder.Property(x => x.JobId)
            .HasColumnName("job_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Job");
        builder.Property(x => x.CronExpression)
            .HasColumnName("cron_expression")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .HasComment("Cron riêng");
        builder.Property(x => x.Parameters)
            .HasColumnName("parameters")
            .HasColumnType("jsonb")
            .HasColumnOrder(4)
            .HasComment("Tham số riêng (merge với mặc định)");
        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bật/tắt cho tenant");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.JobId, x.TenantId }, "uq_job_tenant_overrides").IsUnique();

        builder.HasOne(x => x.Job)
            .WithMany(p => p.JobTenantOverrides)
            .HasForeignKey(x => x.JobId)
            .HasConstraintName("tenant_overrides_job_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("tenant_overrides_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
