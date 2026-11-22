// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.work_calendar_weekdays</c>.
/// </summary>
public sealed class WorkCalendarWeekdayConfiguration : IEntityTypeConfiguration<WorkCalendarWeekday>
{
    public void Configure(EntityTypeBuilder<WorkCalendarWeekday> builder)
    {
        builder.ToTable("work_calendar_weekdays", "mdm", tb =>
        {
            tb.HasComment("Mẫu ngày làm việc trong tuần của lịch (T2..CN, giờ vào/ra, nghỉ trưa)");
            tb.HasCheckConstraint("work_calendar_weekdays_day_of_week_check", "day_of_week BETWEEN 1 AND 7");
            tb.HasCheckConstraint("work_calendar_weekdays_work_ratio_check", "work_ratio BETWEEN 0 AND 1");
        });

        builder.HasKey(x => x.Id).HasName("work_calendar_weekdays_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.CalendarId)
            .HasColumnName("calendar_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Lịch");
        builder.Property(x => x.DayOfWeek)
            .HasColumnName("day_of_week")
            .HasColumnType("smallint")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Thứ theo ISO: 1 = Thứ Hai ... 7 = Chủ Nhật");
        builder.Property(x => x.IsWorkingDay)
            .HasColumnName("is_working_day")
            .HasColumnType("boolean")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Có làm việc không");
        builder.Property(x => x.WorkRatio)
            .HasColumnName("work_ratio")
            .HasColumnType("numeric(3,2)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Tỷ lệ công (1 = cả ngày, 0.5 = nửa ngày T7)");
        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time")
            .HasColumnOrder(5)
            .HasComment("Giờ bắt đầu");
        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time")
            .HasColumnOrder(6)
            .HasComment("Giờ kết thúc");
        builder.Property(x => x.BreakStart)
            .HasColumnName("break_start")
            .HasColumnType("time")
            .HasColumnOrder(7)
            .HasComment("Bắt đầu nghỉ trưa");
        builder.Property(x => x.BreakEnd)
            .HasColumnName("break_end")
            .HasColumnType("time")
            .HasColumnOrder(8)
            .HasComment("Kết thúc nghỉ trưa");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.CalendarId, x.DayOfWeek }, "uq_work_calendar_weekdays").IsUnique();

        builder.HasOne(x => x.Calendar)
            .WithMany(p => p.WorkCalendarWeekdays)
            .HasForeignKey(x => x.CalendarId)
            .HasConstraintName("work_calendar_weekdays_calendar_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
