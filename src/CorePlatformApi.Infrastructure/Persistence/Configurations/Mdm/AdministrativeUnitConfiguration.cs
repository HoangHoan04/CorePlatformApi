// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.administrative_units</c>.
/// </summary>
public sealed class AdministrativeUnitConfiguration : IEntityTypeConfiguration<AdministrativeUnit>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnit> builder)
    {
        builder.ToTable("administrative_units", "mdm", tb =>
        {
            tb.HasComment("Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã). Hỗ trợ cả mô hình 3 cấp cũ và 2 cấp sau sáp nhập 01/07/2025 nhờ effective_from/effective_to");
            tb.HasCheckConstraint("administrative_units_unit_type_check", "unit_type IN ('CENTRAL_CITY','PROVINCE','DISTRICT','URBAN_DISTRICT','TOWN','PROVINCIAL_CITY','WARD','COMMUNE','TOWNSHIP','SPECIAL_ZONE')");
            tb.HasCheckConstraint("administrative_units_level_check", "level BETWEEN 1 AND 3");
        });

        builder.HasKey(x => x.Id).HasName("administrative_units_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("char(2)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Quốc gia");
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Đơn vị cấp trên (NULL nếu là cấp tỉnh)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã đơn vị hành chính (mã GSO/Tổng cục Thống kê)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên ngắn (VD: Hoàn Kiếm)");
        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên đầy đủ (VD: Phường Hoàn Kiếm, Thành phố Hà Nội)");
        builder.Property(x => x.NameEn)
            .HasColumnName("name_en")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(6)
            .HasComment("Tên tiếng Anh / không dấu");
        builder.Property(x => x.UnitType)
            .HasColumnName("unit_type")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Loại đơn vị: TP trực thuộc TW, tỉnh, quận, huyện, thị xã, phường, xã, thị trấn, đặc khu");
        builder.Property(x => x.Level)
            .HasColumnName("level")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .IsRequired()
            .HasComment("Cấp: 1 = tỉnh, 2 = huyện (mô hình cũ) hoặc xã (mô hình 2 cấp), 3 = xã (mô hình cũ)");
        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("Đường dẫn mã từ gốc, VD: '01/00004' để truy vấn cây nhanh");
        builder.Property(x => x.PhoneAreaCode)
            .HasColumnName("phone_area_code")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(10)
            .HasComment("Mã vùng điện thoại cố định (024, 028...)");
        builder.Property(x => x.PostalCode)
            .HasColumnName("postal_code")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(11)
            .HasComment("Mã bưu chính");
        builder.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("numeric(10,7)")
            .HasColumnOrder(12)
            .HasComment("Vĩ độ trung tâm");
        builder.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("numeric(10,7)")
            .HasColumnOrder(13)
            .HasComment("Kinh độ trung tâm");
        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("effective_from")
            .HasColumnType("date")
            .HasColumnOrder(14)
            .HasComment("Ngày bắt đầu hiệu lực (VD: 2025-07-01 cho đơn vị sau sáp nhập)");
        builder.Property(x => x.EffectiveTo)
            .HasColumnName("effective_to")
            .HasColumnType("date")
            .HasColumnOrder(15)
            .HasComment("Ngày hết hiệu lực (đơn vị đã giải thể/sáp nhập); NULL = còn hiệu lực");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng để chọn trên form mới");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.CountryCode, x.Code }, "uq_admin_unit_code").IsUnique();
        builder.HasIndex(x => x.ParentId, "ix_admin_units_parent");
        builder.HasIndex(x => new { x.CountryCode, x.Level }, "ix_admin_units_level")
            .HasFilter("is_active");
        builder.HasIndex(x => x.FullName, "ix_admin_units_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryCode)
            .HasConstraintName("administrative_units_country_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("administrative_units_parent_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
