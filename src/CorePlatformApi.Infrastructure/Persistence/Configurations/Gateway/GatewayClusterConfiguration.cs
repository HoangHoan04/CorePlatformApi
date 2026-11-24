// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho bảng <c>clusters</c>.
/// </summary>
public sealed class GatewayClusterConfiguration : IEntityTypeConfiguration<GatewayCluster>
{
    public void Configure(EntityTypeBuilder<GatewayCluster> builder)
    {
        builder.ToTable("clusters", tb =>
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
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã cluster (hrm-api, wms-api, auth-api)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ");
        builder.Property(x => x.LoadBalancing)
            .HasColumnName("load_balancing")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'ROUND_ROBIN'")
            .ValueGeneratedNever()
            .HasComment("Thuật toán cân bằng tải");
        builder.Property(x => x.HealthCheckPath)
            .HasColumnName("health_check_path")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'/health'")
            .ValueGeneratedNever()
            .HasComment("Đường dẫn health check chủ động");
        builder.Property(x => x.HealthCheckIntervalSeconds)
            .HasColumnName("health_check_interval_seconds")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("15")
            .ValueGeneratedNever()
            .HasComment("Chu kỳ health check");
        builder.Property(x => x.RequestTimeoutMs)
            .HasColumnName("request_timeout_ms")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("30000")
            .ValueGeneratedNever()
            .HasComment("Timeout chuyển tiếp");
        builder.Property(x => x.HttpVersion)
            .HasColumnName("http_version")
            .HasColumnType("varchar(5)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("'1.1'")
            .ValueGeneratedNever()
            .HasComment("Phiên bản HTTP tới upstream");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
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
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
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

        builder.HasIndex(x => x.Code, "clusters_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("clusters_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
