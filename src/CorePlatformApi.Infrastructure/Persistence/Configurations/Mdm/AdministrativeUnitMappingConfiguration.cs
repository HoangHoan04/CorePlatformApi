// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>administrative_unit_mappings</c>.
/// </summary>
public sealed class AdministrativeUnitMappingConfiguration : IEntityTypeConfiguration<AdministrativeUnitMapping>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnitMapping> builder)
    {
        builder.ToTable("administrative_unit_mappings", tb =>
        {
            tb.HasComment("Bảng ánh xạ đơn vị hành chính cũ → mới (sau sáp nhập), dùng để chuyển đổi địa chỉ lịch sử");
            tb.HasCheckConstraint("administrative_unit_mappings_mapping_type_check", "mapping_type IN ('MERGE','SPLIT','RENAME','PARTIAL')");
        });

        builder.HasKey(x => x.Id).HasName("administrative_unit_mappings_pkey");

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
        builder.Property(x => x.OldUnitId)
            .HasColumnName("old_unit_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Đơn vị cũ");
        builder.Property(x => x.NewUnitId)
            .HasColumnName("new_unit_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Đơn vị mới thay thế");
        builder.Property(x => x.MappingType)
            .HasColumnName("mapping_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'MERGE'")
            .ValueGeneratedNever()
            .HasComment("Hình thức: sáp nhập, chia tách, đổi tên, một phần");
        builder.Property(x => x.EffectiveDate)
            .HasColumnName("effective_date")
            .HasColumnType("date")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Ngày có hiệu lực theo nghị quyết");
        builder.Property(x => x.LegalReference)
            .HasColumnName("legal_reference")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(6)
            .HasComment("Số nghị quyết/văn bản pháp lý");
        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Ghi chú");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.OldUnitId, x.NewUnitId }, "uq_admin_unit_mapping").IsUnique();

        builder.HasOne(x => x.OldUnit)
            .WithMany()
            .HasForeignKey(x => x.OldUnitId)
            .HasConstraintName("administrative_unit_mappings_old_unit_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NewUnit)
            .WithMany()
            .HasForeignKey(x => x.NewUnitId)
            .HasConstraintName("administrative_unit_mappings_new_unit_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
