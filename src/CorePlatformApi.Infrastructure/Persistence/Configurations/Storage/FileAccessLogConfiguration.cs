// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho bảng <c>file_access_logs</c>.
/// </summary>
public sealed class FileAccessLogConfiguration : IEntityTypeConfiguration<FileAccessLog>
{
    public void Configure(EntityTypeBuilder<FileAccessLog> builder)
    {
        builder.ToTable("file_access_logs", tb =>
        {
            tb.HasComment("Nhật ký truy cập file nhạy cảm (xem, tải, cấp presigned URL) phục vụ kiểm toán");
            tb.HasCheckConstraint("file_access_logs_action_check", "action IN ('VIEW','DOWNLOAD','PRESIGN','DELETE','SHARE')");
        });

        builder.HasKey(x => x.Id).HasName("file_access_logs_pkey");

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
            .HasComment("Tenant");
        builder.Property(x => x.FileId)
            .HasColumnName("file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("File");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Người truy cập (NULL nếu qua share link)");
        builder.Property(x => x.ShareLinkId)
            .HasColumnName("share_link_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Link chia sẻ được dùng");
        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Hành động");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(6)
            .HasComment("IP");
        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("User-Agent");
        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");
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
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.Id, x.OccurredAt }, "file_access_logs_id_occurred_at_key").IsUnique();
        builder.HasIndex(x => new { x.FileId, x.OccurredAt }, "ix_file_access_logs_file")
            .IsDescending(false, true);
    }
}
