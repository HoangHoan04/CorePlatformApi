// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho <c>notify.event_types</c>.
/// </summary>
public sealed class NotificationEventTypeConfiguration : IEntityTypeConfiguration<NotificationEventType>
{
    public void Configure(EntityTypeBuilder<NotificationEventType> builder)
    {
        builder.ToTable("event_types", "notify", tb =>
        {
            tb.HasComment("Loại sự kiện thông báo do các phân hệ khai báo (hrm.leave_request.submitted, wms.stock.low...)");
            tb.HasCheckConstraint("event_types_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
        });

        builder.HasKey(x => x.Id).HasName("event_types_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Phân hệ phát sinh");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã sự kiện");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên hiển thị trong cài đặt thông báo");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .HasComment("Mô tả");
        builder.Property(x => x.DefaultChannels)
            .HasColumnName("default_channels")
            .HasColumnType("text[]")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'{IN_APP}'")
            .ValueGeneratedNever()
            .HasComment("Kênh mặc định: IN_APP, PUSH, EMAIL, SMS, ZALO");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'NORMAL'")
            .ValueGeneratedNever()
            .HasComment("Mức ưu tiên");
        builder.Property(x => x.IsMandatory)
            .HasColumnName("is_mandatory")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc nhận, user không được tắt (VD: reset mật khẩu)");
        builder.Property(x => x.Variables)
            .HasColumnName("variables")
            .HasColumnType("jsonb")
            .HasColumnOrder(8)
            .HasComment("Khai báo biến template, VD: [{\"name\":\"employee_name\",\"sample\":\"Nguyễn Văn A\"}]");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
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

        builder.HasIndex(x => x.Code, "event_types_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("event_types_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
