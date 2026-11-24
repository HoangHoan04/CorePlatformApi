// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho bảng <c>destinations</c>.
/// </summary>
public sealed class GatewayDestinationConfiguration : IEntityTypeConfiguration<GatewayDestination>
{
    public void Configure(EntityTypeBuilder<GatewayDestination> builder)
    {
        builder.ToTable("destinations", tb =>
        {
            tb.HasComment("Địa chỉ instance cụ thể của cluster (container trong Docker network)");
        });

        builder.HasKey(x => x.Id).HasName("destinations_pkey");

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
        builder.Property(x => x.ClusterId)
            .HasColumnName("cluster_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Cluster");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên instance (hrm-api-1)");
        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Địa chỉ nội bộ (http://hrm-api:8080)");
        builder.Property(x => x.Weight)
            .HasColumnName("weight")
            .HasColumnType("smallint")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("100")
            .ValueGeneratedNever()
            .HasComment("Trọng số");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang nhận traffic");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.ClusterId, x.Name }, "uq_gateway_destinations").IsUnique();

        builder.HasOne(x => x.Cluster)
            .WithMany(p => p.GatewayDestinations)
            .HasForeignKey(x => x.ClusterId)
            .HasConstraintName("destinations_cluster_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
