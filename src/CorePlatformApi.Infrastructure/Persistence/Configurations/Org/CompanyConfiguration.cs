// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Org;

/// <summary>
/// Cấu hình EF Core cho <c>org.companies</c>.
/// </summary>
public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies", "org", tb =>
        {
            tb.HasComment("Công ty con / pháp nhân thuộc tenant. Hỗ trợ công ty mẹ - con nhiều cấp qua parent_id");
            tb.HasCheckConstraint("companies_fiscal_year_start_month_check", "fiscal_year_start_month BETWEEN 1 AND 12");
            tb.HasCheckConstraint("companies_status_check", "status IN ('ACTIVE','INACTIVE')");
        });

        builder.HasKey(x => x.Id).HasName("companies_pkey");

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
            .HasComment("Tenant sở hữu");
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Công ty mẹ (NULL nếu là công ty cấp cao nhất)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã công ty (dùng trong quy tắc sinh mã {COMPANY})");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên giao dịch");
        builder.Property(x => x.LegalName)
            .HasColumnName("legal_name")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .HasComment("Tên pháp lý trên giấy ĐKKD");
        builder.Property(x => x.ShortName)
            .HasColumnName("short_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(6)
            .HasComment("Tên viết tắt");
        builder.Property(x => x.TaxCode)
            .HasColumnName("tax_code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .HasComment("Mã số thuế");
        builder.Property(x => x.BusinessRegNo)
            .HasColumnName("business_reg_no")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(8)
            .HasComment("Số giấy chứng nhận ĐKKD");
        builder.Property(x => x.BusinessRegDate)
            .HasColumnName("business_reg_date")
            .HasColumnType("date")
            .HasColumnOrder(9)
            .HasComment("Ngày cấp ĐKKD");
        builder.Property(x => x.LegalRepresentative)
            .HasColumnName("legal_representative")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(10)
            .HasComment("Người đại diện pháp luật");
        builder.Property(x => x.RepresentativeTitle)
            .HasColumnName("representative_title")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(11)
            .HasComment("Chức danh người đại diện");
        builder.Property(x => x.AddressLine)
            .HasColumnName("address_line")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(12)
            .HasComment("Địa chỉ chi tiết (số nhà, đường)");
        builder.Property(x => x.WardId)
            .HasColumnName("ward_id")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Phường/Xã");
        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Tỉnh/Thành phố");
        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("char(2)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'VN'")
            .ValueGeneratedNever()
            .HasComment("Quốc gia");
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(16)
            .HasComment("Điện thoại");
        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("citext")
            .HasColumnOrder(17)
            .HasComment("Email");
        builder.Property(x => x.Website)
            .HasColumnName("website")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(18)
            .HasComment("Website");
        builder.Property(x => x.LogoFileId)
            .HasColumnName("logo_file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Logo (storage.files)");
        builder.Property(x => x.BaseCurrencyCode)
            .HasColumnName("base_currency_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("'VND'")
            .ValueGeneratedNever()
            .HasComment("Tiền tệ hạch toán");
        builder.Property(x => x.FiscalYearStartMonth)
            .HasColumnName("fiscal_year_start_month")
            .HasColumnType("smallint")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Tháng bắt đầu năm tài chính");
        builder.Property(x => x.WorkCalendarId)
            .HasColumnName("work_calendar_id")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Lịch làm việc mặc định (mdm.work_calendars)");
        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasColumnType("text")
            .HasColumnOrder(23)
            .HasComment("Đường dẫn id từ gốc '/uuid1/uuid2/' để truy vấn cây");
        builder.Property(x => x.Level)
            .HasColumnName("level")
            .HasColumnType("smallint")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Cấp trong cây công ty");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(25)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(26)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái hoạt động");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(27)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(28)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(29)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(30)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(31)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(32)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(33)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => x.ParentId, "ix_companies_parent");
        builder.HasIndex(x => x.Path, "ix_companies_path")
            .HasOperators("text_pattern_ops");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("companies_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("companies_parent_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Ward)
            .WithMany()
            .HasForeignKey(x => x.WardId)
            .HasConstraintName("companies_ward_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .HasConstraintName("companies_province_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryCode)
            .HasConstraintName("companies_country_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.BaseCurrency)
            .WithMany()
            .HasForeignKey(x => x.BaseCurrencyCode)
            .HasConstraintName("companies_base_currency_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.LogoFile)
            .WithMany()
            .HasForeignKey(x => x.LogoFileId)
            .HasConstraintName("fk_companies_logo")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.WorkCalendar)
            .WithMany()
            .HasForeignKey(x => x.WorkCalendarId)
            .HasConstraintName("fk_companies_calendar")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
