// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>holidays</c>.
/// </summary>
public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holidays", tb =>
        {
            tb.HasComment("Ngày lễ quốc gia (tenant_id NULL) và ngày lễ/nghỉ riêng của tenant");
            tb.HasCheckConstraint("holidays_holiday_type_check", "holiday_type IN ('PUBLIC','COMPENSATORY','COMPANY','OBSERVANCE')");
        });

        builder.HasKey(x => x.Id).HasName("holidays_pkey");

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
            .HasComment("Tenant (NULL = lễ quốc gia)");
        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("char(2)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasDefaultValueSql("'VN'")
            .ValueGeneratedNever()
            .HasComment("Quốc gia");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(3)
            .HasComment("Mã ngày lễ (TET, NATIONAL_DAY, LABOR_DAY)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên ngày lễ");
        builder.Property(x => x.HolidayDate)
            .HasColumnName("holiday_date")
            .HasColumnType("date")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Ngày dương lịch cụ thể (đã quy đổi nếu là âm lịch)");
        builder.Property(x => x.Year)
            .HasColumnName("year")
            .HasColumnType("smallint")
            .HasColumnOrder(6)
            .HasComputedColumnSql("EXTRACT(YEAR FROM holiday_date)::smallint", stored: true)
            .HasComment("Năm (tính tự động để lọc nhanh)");
        builder.Property(x => x.IsLunar)
            .HasColumnName("is_lunar")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Gốc là ngày âm lịch (Tết, Giỗ Tổ)");
        builder.Property(x => x.LunarDay)
            .HasColumnName("lunar_day")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .HasComment("Ngày âm");
        builder.Property(x => x.LunarMonth)
            .HasColumnName("lunar_month")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .HasComment("Tháng âm");
        builder.Property(x => x.HolidayType)
            .HasColumnName("holiday_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'PUBLIC'")
            .ValueGeneratedNever()
            .HasComment("Loại: lễ chính thức, nghỉ bù, nghỉ công ty, ngày kỷ niệm (không nghỉ)");
        builder.Property(x => x.IsDayOff)
            .HasColumnName("is_day_off")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Có được nghỉ không");
        builder.Property(x => x.IsPaid)
            .HasColumnName("is_paid")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Nghỉ hưởng lương (HRM tính công)");
        builder.Property(x => x.PayMultiplier)
            .HasColumnName("pay_multiplier")
            .HasColumnType("numeric(5,2)")
            .HasColumnOrder(13)
            .HasDefaultValueSql("3.00")
            .ValueGeneratedNever()
            .HasComment("Hệ số lương nếu đi làm ngày này (BLLĐ: tối thiểu 300%)");
        builder.Property(x => x.LegalReference)
            .HasColumnName("legal_reference")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(14)
            .HasComment("Văn bản quy định (thông báo của Bộ LĐ)");
        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasColumnType("text")
            .HasColumnOrder(15)
            .HasComment("Ghi chú");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.CountryCode, x.HolidayDate }, "ix_holidays_date")
            .HasFilter("NOT is_deleted");
        builder.HasIndex(x => new { x.TenantId, x.Year }, "ix_holidays_tenant_year")
            .HasFilter("NOT is_deleted");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("holidays_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("holidays_country_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
