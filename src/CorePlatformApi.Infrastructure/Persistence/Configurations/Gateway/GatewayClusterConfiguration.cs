// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho <c>gateway.clusters</c>.
/// </summary>
public sealed class GatewayClusterConfiguration : IEntityTypeConfiguration<GatewayCluster>
{
    public void Configure(EntityTypeBuilder<GatewayCluster> builder)
    {
        builder.ToTable("clusters", "gateway", tb =>
        {
            tb.HasComment("Cụm upstream (service nội bộ) mà gateway định tuyến tới");
            tb.HasCheckConstraint("clusters_load_balancing_check", "load_balancing IN ('ROUND_ROBIN','LEAST_REQUESTS','RANDOM','POWER_OF_TWO','FIRST')");
        });

        builder.HasKey(x => x.Id).HasName("clusters_pkey");

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
            .HasComment("Mã cluster (hrm-api, wms-api, auth-api)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Phân hệ");
        builder.Property(x => x.LoadBalancing)
            .HasColumnName("load_balancing")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'ROUND_ROBIN'")
            .ValueGeneratedNever()
            .HasComment("Thuật toán cân bằng tải");
        builder.Property(x => x.HealthCheckPath)
            .HasColumnName("health_check_path")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'/health'")
            .ValueGeneratedNever()
            .HasComment("Đường dẫn health check chủ động");
        builder.Property(x => x.HealthCheckIntervalSeconds)
            .HasColumnName("health_check_interval_seconds")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("15")
            .ValueGeneratedNever()
            .HasComment("Chu kỳ health check");
        builder.Property(x => x.RequestTimeoutMs)
            .HasColumnName("request_timeout_ms")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("30000")
            .ValueGeneratedNever()
            .HasComment("Timeout chuyển tiếp");
        builder.Property(x => x.HttpVersion)
            .HasColumnName("http_version")
            .HasColumnType("varchar(5)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'1.1'")
            .ValueGeneratedNever()
            .HasComment("Phiên bản HTTP tới upstream");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
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
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.Code, "clusters_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("clusters_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
