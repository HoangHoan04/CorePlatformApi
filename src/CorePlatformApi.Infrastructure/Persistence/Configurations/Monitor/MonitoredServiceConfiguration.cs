// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Monitor;

/// <summary>
/// Cấu hình EF Core cho <c>monitor.services</c>.
/// </summary>
public sealed class MonitoredServiceConfiguration : IEntityTypeConfiguration<MonitoredService>
{
    public void Configure(EntityTypeBuilder<MonitoredService> builder)
    {
        builder.ToTable("services", "monitor", tb =>
        {
            tb.HasComment("Danh mục service trong hệ sinh thái và endpoint /health để Docker, Reverse Proxy, Uptime Monitor kiểm tra");
            tb.HasCheckConstraint("services_service_type_check", "service_type IN ('API','WORKER','WEB','DATABASE','CACHE','QUEUE','STORAGE','EXTERNAL')");
            tb.HasCheckConstraint("services_environment_check", "environment IN ('DEVELOPMENT','STAGING','PRODUCTION')");
            tb.HasCheckConstraint("services_current_status_check", "current_status IN ('HEALTHY','DEGRADED','UNHEALTHY','UNKNOWN','MAINTENANCE')");
        });

        builder.HasKey(x => x.Id).HasName("services_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã service (auth-api, hrm-api, core-worker, postgres, redis, rabbitmq, minio)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ");
        builder.Property(x => x.ServiceType)
            .HasColumnName("service_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'API'")
            .ValueGeneratedNever()
            .HasComment("Loại");
        builder.Property(x => x.Environment)
            .HasColumnName("environment")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'PRODUCTION'")
            .ValueGeneratedNever()
            .HasComment("Môi trường");
        builder.Property(x => x.BaseUrl)
            .HasColumnName("base_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(6)
            .HasComment("URL nội bộ");
        builder.Property(x => x.HealthUrl)
            .HasColumnName("health_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(7)
            .HasComment("URL health (/health/ready)");
        builder.Property(x => x.LivenessUrl)
            .HasColumnName("liveness_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("URL liveness (/health/live)");
        builder.Property(x => x.CheckIntervalSeconds)
            .HasColumnName("check_interval_seconds")
            .HasColumnType("int")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("30")
            .ValueGeneratedNever()
            .HasComment("Chu kỳ kiểm tra");
        builder.Property(x => x.TimeoutMs)
            .HasColumnName("timeout_ms")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("5000")
            .ValueGeneratedNever()
            .HasComment("Timeout");
        builder.Property(x => x.AlertAfterFailures)
            .HasColumnName("alert_after_failures")
            .HasColumnType("smallint")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("3")
            .ValueGeneratedNever()
            .HasComment("Cảnh báo sau N lần lỗi liên tiếp");
        builder.Property(x => x.AlertChannels)
            .HasColumnName("alert_channels")
            .HasColumnType("jsonb")
            .HasColumnOrder(12)
            .HasComment("Kênh cảnh báo (email, Telegram, Slack webhook)");
        builder.Property(x => x.CurrentVersion)
            .HasColumnName("current_version")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(13)
            .HasComment("Phiên bản đang chạy (đọc từ /health)");
        builder.Property(x => x.CurrentStatus)
            .HasColumnName("current_status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("'UNKNOWN'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái hiện tại");
        builder.Property(x => x.ConsecutiveFailures)
            .HasColumnName("consecutive_failures")
            .HasColumnType("int")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần lỗi liên tiếp");
        builder.Property(x => x.LastCheckedAt)
            .HasColumnName("last_checked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Kiểm tra gần nhất");
        builder.Property(x => x.LastStatusChangeAt)
            .HasColumnName("last_status_change_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Đổi trạng thái gần nhất");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang giám sát");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.Code, "services_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("services_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
