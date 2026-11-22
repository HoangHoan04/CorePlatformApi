// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.external_mappings</c>.
/// </summary>
public sealed class ExternalMappingConfiguration : IEntityTypeConfiguration<ExternalMapping>
{
    public void Configure(EntityTypeBuilder<ExternalMapping> builder)
    {
        builder.ToTable("external_mappings", "integration", tb =>
        {
            tb.HasComment("Ánh xạ id nội bộ ↔ id hệ thống ngoài (sản phẩm ERP ↔ item_id Shopee, NV ↔ mã trên máy chấm công)");
        });

        builder.HasKey(x => x.Id).HasName("external_mappings_pkey");

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
        builder.Property(x => x.ConnectionId)
            .HasColumnName("connection_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Kết nối");
        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Loại đối tượng nội bộ (PRODUCT, EMPLOYEE, CUSTOMER)");
        builder.Property(x => x.InternalId)
            .HasColumnName("internal_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Id nội bộ");
        builder.Property(x => x.ExternalSystem)
            .HasColumnName("external_system")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Hệ thống ngoài");
        builder.Property(x => x.ExternalId)
            .HasColumnName("external_id")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Id phía ngoài");
        builder.Property(x => x.ExternalData)
            .HasColumnName("external_data")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .HasComment("Dữ liệu bổ sung phía ngoài");
        builder.Property(x => x.LastSyncedAt)
            .HasColumnName("last_synced_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Đồng bộ gần nhất");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Tạo lúc");

        builder.HasIndex(x => new { x.TenantId, x.ExternalSystem, x.EntityType, x.ExternalId }, "uq_external_mapping_ext").IsUnique();
        builder.HasIndex(x => new { x.EntityType, x.InternalId }, "ix_external_mappings_internal");

        builder.HasOne(x => x.Connection)
            .WithMany(p => p.ExternalMappings)
            .HasForeignKey(x => x.ConnectionId)
            .HasConstraintName("external_mappings_connection_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
