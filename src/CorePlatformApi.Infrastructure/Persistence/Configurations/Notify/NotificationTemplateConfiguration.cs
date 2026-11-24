// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho bảng <c>templates</c>.
/// </summary>
public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("templates", tb =>
        {
            tb.HasComment("Mẫu nội dung theo sự kiện × kênh × ngôn ngữ, dùng biến {{employee_name}}. Tenant có thể ghi đè mẫu hệ thống");
            tb.HasCheckConstraint("templates_channel_check", "channel IN ('IN_APP','PUSH','EMAIL','SMS','ZALO')");
            tb.HasCheckConstraint("templates_body_format_check", "body_format IN ('TEXT','HTML','MARKDOWN')");
        });

        builder.HasKey(x => x.Id).HasName("templates_pkey");

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
            .HasComment("Tenant (NULL = mẫu hệ thống)");
        builder.Property(x => x.EventTypeId)
            .HasColumnName("event_type_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Sự kiện");
        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Kênh");
        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'vi-VN'")
            .ValueGeneratedNever()
            .HasComment("Ngôn ngữ");
        builder.Property(x => x.Subject)
            .HasColumnName("subject")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .HasComment("Tiêu đề (email) / title (push, in-app)");
        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Nội dung có biến {{...}}");
        builder.Property(x => x.BodyFormat)
            .HasColumnName("body_format")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'TEXT'")
            .ValueGeneratedNever()
            .HasComment("Định dạng nội dung");
        builder.Property(x => x.ActionUrl)
            .HasColumnName("action_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Link mở khi bấm (có biến, VD: /hrm/leave-requests/{{id}})");
        builder.Property(x => x.DeepLink)
            .HasColumnName("deep_link")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(9)
            .HasComment("Deep link mobile app");
        builder.Property(x => x.ExternalTemplateId)
            .HasColumnName("external_template_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(10)
            .HasComment("Mã template phía nhà cung cấp (Zalo ZNS, SMS brandname)");
        builder.Property(x => x.TemplateVersion)
            .HasColumnName("version")
            .HasColumnType("int")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Phiên bản");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.EventTypeId, x.Channel, x.Locale }, "uq_notify_templates")
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("is_active AND NOT is_deleted");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("templates_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.EventType)
            .WithMany()
            .HasForeignKey(x => x.EventTypeId)
            .HasConstraintName("templates_event_type_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
