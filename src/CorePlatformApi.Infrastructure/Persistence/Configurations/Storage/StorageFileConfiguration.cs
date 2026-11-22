// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho <c>storage.files</c>.
/// </summary>
public sealed class StorageFileConfiguration : IEntityTypeConfiguration<StorageFile>
{
    public void Configure(EntityTypeBuilder<StorageFile> builder)
    {
        builder.ToTable("files", "storage", tb =>
        {
            tb.HasComment("Metadata file đã upload. File vật lý nằm trên Object Storage; mọi phân hệ chỉ lưu file_id");
            tb.HasCheckConstraint("files_size_bytes_check", "size_bytes >= 0");
            tb.HasCheckConstraint("files_sensitivity_check", "sensitivity IN ('PUBLIC','INTERNAL','CONFIDENTIAL','RESTRICTED')");
            tb.HasCheckConstraint("files_status_check", "status IN ('UPLOADING','TEMP','ACTIVE','QUARANTINED','DELETED')");
            tb.HasCheckConstraint("files_scan_status_check", "scan_status IN ('PENDING','CLEAN','INFECTED','SKIPPED','ERROR')");
            tb.HasCheckConstraint("files_upload_source_check", "upload_source IN ('WEB','MOBILE','API','SYSTEM','IMPORT')");
        });

        builder.HasKey(x => x.Id).HasName("files_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (file_id dùng ở các phân hệ)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant");
        builder.Property(x => x.BucketId)
            .HasColumnName("bucket_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Bucket");
        builder.Property(x => x.ObjectKey)
            .HasColumnName("object_key")
            .HasColumnType("varchar(1000)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Đường dẫn object trên storage");
        builder.Property(x => x.OriginalName)
            .HasColumnName("original_name")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên file gốc người dùng upload");
        builder.Property(x => x.Extension)
            .HasColumnName("extension")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .HasComment("Đuôi file (.pdf)");
        builder.Property(x => x.MimeType)
            .HasColumnName("mime_type")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("MIME type đã kiểm tra bằng magic bytes");
        builder.Property(x => x.SizeBytes)
            .HasColumnName("size_bytes")
            .HasColumnType("bigint")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Dung lượng (byte)");
        builder.Property(x => x.ChecksumSha256)
            .HasColumnName("checksum_sha256")
            .HasColumnType("char(64)")
            .HasColumnOrder(8)
            .HasComment("SHA-256 phát hiện trùng/toàn vẹn");
        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'ATTACHMENT'")
            .ValueGeneratedNever()
            .HasComment("Loại file (theo storage.upload_policies.category)");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(10)
            .HasComment("Phân hệ sở hữu (HRM, WMS...)");
        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(11)
            .HasComment("Loại đối tượng gắn (EMPLOYEE, LEAVE_REQUEST, GOODS_RECEIPT)");
        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Id đối tượng gắn");
        builder.Property(x => x.FieldName)
            .HasColumnName("field_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(13)
            .HasComment("Trường của đối tượng (avatar, contract_scan, attachments)");
        builder.Property(x => x.Sensitivity)
            .HasColumnName("sensitivity")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("'INTERNAL'")
            .ValueGeneratedNever()
            .HasComment("Mức nhạy cảm");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'TEMP'")
            .ValueGeneratedNever()
            .HasComment("TEMP: đã upload chưa gắn entity; ACTIVE: đã gắn; QUARANTINED: nghi nhiễm virus");
        builder.Property(x => x.ScanStatus)
            .HasColumnName("scan_status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Kết quả quét virus");
        builder.Property(x => x.Width)
            .HasColumnName("width")
            .HasColumnType("int")
            .HasColumnOrder(17)
            .HasComment("Chiều rộng ảnh (px)");
        builder.Property(x => x.Height)
            .HasColumnName("height")
            .HasColumnType("int")
            .HasColumnOrder(18)
            .HasComment("Chiều cao ảnh (px)");
        builder.Property(x => x.DurationSeconds)
            .HasColumnName("duration_seconds")
            .HasColumnType("numeric(10,2)")
            .HasColumnOrder(19)
            .HasComment("Thời lượng (video/audio)");
        builder.Property(x => x.PageCount)
            .HasColumnName("page_count")
            .HasColumnType("int")
            .HasColumnOrder(20)
            .HasComment("Số trang (PDF)");
        builder.Property(x => x.VersionNo)
            .HasColumnName("version_no")
            .HasColumnType("int")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Phiên bản của tài liệu");
        builder.Property(x => x.PreviousVersionId)
            .HasColumnName("previous_version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Phiên bản trước");
        builder.Property(x => x.UploadSource)
            .HasColumnName("upload_source")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(23)
            .IsRequired()
            .HasDefaultValueSql("'WEB'")
            .ValueGeneratedNever()
            .HasComment("Nguồn upload");
        builder.Property(x => x.UploadedBy)
            .HasColumnName("uploaded_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("User upload");
        builder.Property(x => x.UploadedIp)
            .HasColumnName("uploaded_ip")
            .HasColumnType("inet")
            .HasColumnOrder(25)
            .HasComment("IP upload");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(26)
            .HasComment("File TEMP hết hạn sẽ bị job dọn");
        builder.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasColumnOrder(27)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Metadata mở rộng (EXIF đã lọc, tag...)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(28)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(29)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(30)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(31)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(32)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(33)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(34)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.ModuleCode, x.EntityType, x.EntityId }, "ix_files_entity")
            .HasFilter("NOT is_deleted");
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt }, "ix_files_tenant_created")
            .IsDescending(false, true);
        builder.HasIndex(x => x.ExpiresAt, "ix_files_temp_expires")
            .HasFilter("status = 'TEMP'");
        builder.HasIndex(x => new { x.TenantId, x.ChecksumSha256 }, "ix_files_checksum");
        builder.HasIndex(x => new { x.BucketId, x.ObjectKey }, "uq_files_object")
            .IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("files_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Bucket)
            .WithMany()
            .HasForeignKey(x => x.BucketId)
            .HasConstraintName("files_bucket_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.PreviousVersion)
            .WithMany()
            .HasForeignKey(x => x.PreviousVersionId)
            .HasConstraintName("files_previous_version_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
