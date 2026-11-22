// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Org;

/// <summary>
/// Cấu hình EF Core cho <c>org.branches</c>.
/// </summary>
public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches", "org", tb =>
        {
            tb.HasComment("Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty");
            tb.HasCheckConstraint("branches_branch_type_check", "branch_type IN ('HEAD_OFFICE','OFFICE','STORE','WAREHOUSE','FACTORY','REGION','OTHER')");
            tb.HasCheckConstraint("branches_status_check", "status IN ('ACTIVE','INACTIVE','CLOSED')");
        });

        builder.HasKey(x => x.Id).HasName("branches_pkey");

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
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Công ty sở hữu chi nhánh");
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Chi nhánh cấp trên (VD: vùng → cửa hàng)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Mã chi nhánh (dùng trong sinh mã {BRANCH}, VD: HN01)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên chi nhánh");
        builder.Property(x => x.BranchType)
            .HasColumnName("branch_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'OFFICE'")
            .ValueGeneratedNever()
            .HasComment("Loại: trụ sở, văn phòng, cửa hàng, kho, nhà máy, vùng");
        builder.Property(x => x.TaxCode)
            .HasColumnName("tax_code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .HasComment("MST chi nhánh (nếu hạch toán phụ thuộc có MST 13 số)");
        builder.Property(x => x.AddressLine)
            .HasColumnName("address_line")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Địa chỉ chi tiết");
        builder.Property(x => x.WardId)
            .HasColumnName("ward_id")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Phường/Xã");
        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Tỉnh/Thành phố");
        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("char(2)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'VN'")
            .ValueGeneratedNever()
            .HasComment("Quốc gia");
        builder.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("numeric(10,7)")
            .HasColumnOrder(12)
            .HasComment("Vĩ độ (dùng cho chấm công GPS, giao hàng)");
        builder.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("numeric(10,7)")
            .HasColumnOrder(13)
            .HasComment("Kinh độ");
        builder.Property(x => x.GeofenceRadiusM)
            .HasColumnName("geofence_radius_m")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .HasComment("Bán kính geofence (mét) cho chấm công mobile");
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(15)
            .HasComment("Điện thoại");
        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("citext")
            .HasColumnOrder(16)
            .HasComment("Email");
        builder.Property(x => x.ManagerUserId)
            .HasColumnName("manager_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người quản lý chi nhánh (iam.users)");
        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(18)
            .HasComment("Múi giờ riêng (NULL = theo tenant)");
        builder.Property(x => x.WorkCalendarId)
            .HasColumnName("work_calendar_id")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Lịch làm việc riêng (NULL = theo công ty)");
        builder.Property(x => x.OpeningDate)
            .HasColumnName("opening_date")
            .HasColumnType("date")
            .HasColumnOrder(20)
            .HasComment("Ngày khai trương/thành lập");
        builder.Property(x => x.ClosingDate)
            .HasColumnName("closing_date")
            .HasColumnType("date")
            .HasColumnOrder(21)
            .HasComment("Ngày đóng cửa");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(23)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(25)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(26)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(27)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(28)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(29)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(30)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => x.CompanyId, "ix_branches_company")
            .HasFilter("NOT is_deleted");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("branches_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("branches_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("branches_parent_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Ward)
            .WithMany()
            .HasForeignKey(x => x.WardId)
            .HasConstraintName("branches_ward_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .HasConstraintName("branches_province_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryCode)
            .HasConstraintName("branches_country_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ManagerUser)
            .WithMany()
            .HasForeignKey(x => x.ManagerUserId)
            .HasConstraintName("fk_branches_manager")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.WorkCalendar)
            .WithMany()
            .HasForeignKey(x => x.WorkCalendarId)
            .HasConstraintName("fk_branches_calendar")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
