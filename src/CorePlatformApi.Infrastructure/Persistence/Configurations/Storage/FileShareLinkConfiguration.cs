// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho bảng <c>file_share_links</c>.
/// </summary>
public sealed class FileShareLinkConfiguration : IEntityTypeConfiguration<FileShareLink>
{
    public void Configure(EntityTypeBuilder<FileShareLink> builder)
    {
        builder.ToTable("file_share_links", tb =>
        {
            tb.HasComment("Link chia sẻ file có kiểm soát (hết hạn, giới hạn lượt tải, mật khẩu) — khác presigned URL ngắn hạn của S3");
        });

        builder.HasKey(x => x.Id).HasName("file_share_links_pkey");

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
        builder.Property(x => x.FileId)
            .HasColumnName("file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("File");
        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(128)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Hash token trong URL chia sẻ");
        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .HasComment("Mật khẩu mở link (tùy chọn)");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Hết hạn");
        builder.Property(x => x.MaxDownloads)
            .HasColumnName("max_downloads")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .HasComment("Số lượt tải tối đa");
        builder.Property(x => x.DownloadCount)
            .HasColumnName("download_count")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Đã tải");
        builder.Property(x => x.AllowedEmails)
            .HasColumnName("allowed_emails")
            .HasColumnType("citext[]")
            .HasColumnOrder(8)
            .HasComment("Chỉ cho phép email này (tùy chọn)");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Thu hồi lúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo link");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (users.id)");
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
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.TokenHash, "file_share_links_token_hash_key").IsUnique();

        builder.HasOne(x => x.File)
            .WithMany(p => p.FileShareLinks)
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("file_share_links_file_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
