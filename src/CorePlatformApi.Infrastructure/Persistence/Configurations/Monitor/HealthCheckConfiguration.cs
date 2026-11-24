// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Monitor;

/// <summary>
/// Cấu hình EF Core cho bảng <c>health_checks</c>.
/// </summary>
public sealed class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("health_checks", tb =>
        {
            tb.HasComment("Kết quả từng lần health check");
            tb.HasCheckConstraint("health_checks_status_check", "status IN ('HEALTHY','DEGRADED','UNHEALTHY')");
        });

        builder.HasKey(x => x.Id).HasName("health_checks_pkey");

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
        builder.Property(x => x.ServiceId)
            .HasColumnName("service_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Service");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Kết quả");
        builder.Property(x => x.HttpStatus)
            .HasColumnName("http_status")
            .HasColumnType("smallint")
            .HasColumnOrder(4)
            .HasComment("HTTP status");
        builder.Property(x => x.ResponseMs)
            .HasColumnName("response_ms")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .HasComment("Thời gian phản hồi");
        builder.Property(x => x.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .HasColumnOrder(6)
            .HasComment("Chi tiết từng dependency (db, redis, rabbitmq) trả về từ /health");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Lỗi");
        builder.Property(x => x.CheckedAt)
            .HasColumnName("checked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.Id, x.CheckedAt }, "health_checks_id_checked_at_key").IsUnique();
        builder.HasIndex(x => new { x.ServiceId, x.CheckedAt }, "ix_health_checks_service")
            .IsDescending(false, true);
    }
}
