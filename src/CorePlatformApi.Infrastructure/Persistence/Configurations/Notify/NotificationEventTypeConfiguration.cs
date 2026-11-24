// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho bảng <c>notify_event_types</c>.
/// </summary>
public sealed class NotificationEventTypeConfiguration : IEntityTypeConfiguration<NotificationEventType>
{
    public void Configure(EntityTypeBuilder<NotificationEventType> builder)
    {
        builder.ToTable("notify_event_types", tb =>
        {
            tb.HasComment("Loại sự kiện thông báo do các phân hệ khai báo (hrm.leave_request.submitted, wms.stock.low...)");
            tb.HasCheckConstraint("notify_event_types_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
        });

        builder.HasKey(x => x.Id).HasName("notify_event_types_pkey");

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
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phân hệ phát sinh");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã sự kiện");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên hiển thị trong cài đặt thông báo");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.DefaultChannels)
            .HasColumnName("default_channels")
            .HasColumnType("text[]")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'{IN_APP}'")
            .ValueGeneratedNever()
            .HasComment("Kênh mặc định: IN_APP, PUSH, EMAIL, SMS, ZALO");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'NORMAL'")
            .ValueGeneratedNever()
            .HasComment("Mức ưu tiên");
        builder.Property(x => x.IsMandatory)
            .HasColumnName("is_mandatory")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc nhận, user không được tắt (VD: reset mật khẩu)");
        builder.Property(x => x.Variables)
            .HasColumnName("variables")
            .HasColumnType("jsonb")
            .HasColumnOrder(9)
            .HasComment("Khai báo biến template, VD: [{\"name\":\"employee_name\",\"sample\":\"Nguyễn Văn A\"}]");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
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
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Code, "notify_event_types_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("notify_event_types_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
