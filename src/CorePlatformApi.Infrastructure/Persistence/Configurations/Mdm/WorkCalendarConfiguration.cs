// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>work_calendars</c>.
/// </summary>
public sealed class WorkCalendarConfiguration : IEntityTypeConfiguration<WorkCalendar>
{
    public void Configure(EntityTypeBuilder<WorkCalendar> builder)
    {
        builder.ToTable("work_calendars", tb =>
        {
            tb.HasComment("Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc");
            tb.HasCheckConstraint("work_calendars_status_check", "status IN ('ACTIVE','INACTIVE')");
        });

        builder.HasKey(x => x.Id).HasName("work_calendars_pkey");

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
            .HasComment("Công ty áp dụng (NULL = toàn tenant)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã lịch (HC_T2_T6, HC_T2_T7_SANG)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên lịch");
        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'Asia/Ho_Chi_Minh'")
            .ValueGeneratedNever()
            .HasComment("Múi giờ");
        builder.Property(x => x.StandardHoursPerDay)
            .HasColumnName("standard_hours_per_day")
            .HasColumnType("numeric(4,2)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("8")
            .ValueGeneratedNever()
            .HasComment("Số giờ công chuẩn/ngày");
        builder.Property(x => x.ApplyPublicHolidays)
            .HasColumnName("apply_public_holidays")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Tự áp dụng lễ quốc gia từ mdm.holidays");
        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Lịch mặc định của tenant");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
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
            .HasComment("Người xóa mềm (iam.users.id)");


        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("work_calendars_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("work_calendars_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
