// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>work_calendar_exceptions</c>.
/// </summary>
public sealed class WorkCalendarExceptionConfiguration : IEntityTypeConfiguration<WorkCalendarException>
{
    public void Configure(EntityTypeBuilder<WorkCalendarException> builder)
    {
        builder.ToTable("work_calendar_exceptions", tb =>
        {
            tb.HasComment("Ngoại lệ của lịch: nghỉ riêng công ty, ngày làm bù, ngày đổi ca... áp dụng cho cả lịch hoặc riêng chi nhánh");
            tb.HasCheckConstraint("work_calendar_exceptions_exception_type_check", "exception_type IN ('DAY_OFF','MAKEUP_WORKDAY','HALF_DAY','CUSTOM_HOURS')");
        });

        builder.HasKey(x => x.Id).HasName("work_calendar_exceptions_pkey");

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
        builder.Property(x => x.CalendarId)
            .HasColumnName("calendar_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Lịch");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Chỉ áp dụng cho chi nhánh này (NULL = toàn lịch)");
        builder.Property(x => x.ExceptionDate)
            .HasColumnName("exception_date")
            .HasColumnType("date")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Ngày ngoại lệ");
        builder.Property(x => x.ExceptionType)
            .HasColumnName("exception_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Loại: nghỉ, làm bù, nửa ngày, giờ đặc biệt");
        builder.Property(x => x.HolidayId)
            .HasColumnName("holiday_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Liên kết ngày lễ (nếu do lễ)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Tên/lý do");
        builder.Property(x => x.IsPaid)
            .HasColumnName("is_paid")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Nghỉ có lương");
        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time")
            .HasColumnOrder(9)
            .HasComment("Giờ bắt đầu (CUSTOM_HOURS)");
        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time")
            .HasColumnOrder(10)
            .HasComment("Giờ kết thúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.CalendarId, x.ExceptionDate, x.BranchId }, "uq_work_calendar_exceptions")
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("NOT is_deleted");

        builder.HasOne(x => x.Calendar)
            .WithMany(p => p.WorkCalendarExceptions)
            .HasForeignKey(x => x.CalendarId)
            .HasConstraintName("work_calendar_exceptions_calendar_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("work_calendar_exceptions_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Holiday)
            .WithMany()
            .HasForeignKey(x => x.HolidayId)
            .HasConstraintName("work_calendar_exceptions_holiday_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
