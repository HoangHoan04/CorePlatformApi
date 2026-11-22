// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Monitor;

/// <summary>
/// Cấu hình EF Core cho <c>monitor.health_checks</c>.
/// </summary>
public sealed class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("health_checks", "monitor", tb =>
        {
            tb.HasComment("Kết quả từng lần health check");
            tb.HasCheckConstraint("health_checks_status_check", "status IN ('HEALTHY','DEGRADED','UNHEALTHY')");
            tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql
        });

        builder.HasKey(x => new { x.Id, x.CheckedAt }).HasName("health_checks_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.ServiceId)
            .HasColumnName("service_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Service");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Kết quả");
        builder.Property(x => x.HttpStatus)
            .HasColumnName("http_status")
            .HasColumnType("smallint")
            .HasColumnOrder(3)
            .HasComment("HTTP status");
        builder.Property(x => x.ResponseMs)
            .HasColumnName("response_ms")
            .HasColumnType("int")
            .HasColumnOrder(4)
            .HasComment("Thời gian phản hồi");
        builder.Property(x => x.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .HasComment("Chi tiết từng dependency (db, redis, rabbitmq) trả về từ /health");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Lỗi");
        builder.Property(x => x.CheckedAt)
            .HasColumnName("checked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");

    }
}
