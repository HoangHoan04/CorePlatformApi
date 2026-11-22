// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.administrative_unit_mappings</c>.
/// </summary>
public sealed class AdministrativeUnitMappingConfiguration : IEntityTypeConfiguration<AdministrativeUnitMapping>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnitMapping> builder)
    {
        builder.ToTable("administrative_unit_mappings", "mdm", tb =>
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
        builder.Property(x => x.OldUnitId)
            .HasColumnName("old_unit_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Đơn vị cũ");
        builder.Property(x => x.NewUnitId)
            .HasColumnName("new_unit_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Đơn vị mới thay thế");
        builder.Property(x => x.MappingType)
            .HasColumnName("mapping_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'MERGE'")
            .ValueGeneratedNever()
            .HasComment("Hình thức: sáp nhập, chia tách, đổi tên, một phần");
        builder.Property(x => x.EffectiveDate)
            .HasColumnName("effective_date")
            .HasColumnType("date")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Ngày có hiệu lực theo nghị quyết");
        builder.Property(x => x.LegalReference)
            .HasColumnName("legal_reference")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .HasComment("Số nghị quyết/văn bản pháp lý");
        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Ghi chú");
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
