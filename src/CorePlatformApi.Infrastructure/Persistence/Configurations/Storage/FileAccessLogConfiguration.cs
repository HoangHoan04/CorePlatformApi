// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho <c>storage.file_access_logs</c>.
/// </summary>
public sealed class FileAccessLogConfiguration : IEntityTypeConfiguration<FileAccessLog>
{
    public void Configure(EntityTypeBuilder<FileAccessLog> builder)
    {
        builder.ToTable("file_access_logs", "storage", tb =>
        {
            tb.HasComment("Nhật ký truy cập file nhạy cảm (xem, tải, cấp presigned URL) phục vụ kiểm toán");
            tb.HasCheckConstraint("file_access_logs_action_check", "action IN ('VIEW','DOWNLOAD','PRESIGN','DELETE','SHARE')");
            tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql
        });

        builder.HasKey(x => new { x.Id, x.OccurredAt }).HasName("file_access_logs_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.FileId)
            .HasColumnName("file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("File");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Tenant");
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

    }
}
