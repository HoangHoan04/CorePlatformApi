// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho bảng <c>upload_policies</c>.
/// </summary>
public sealed class UploadPolicyConfiguration : IEntityTypeConfiguration<UploadPolicy>
{
    public void Configure(EntityTypeBuilder<UploadPolicy> builder)
    {
        builder.ToTable("upload_policies", tb =>
        {
            tb.HasComment("Chính sách upload theo loại file: dung lượng, định dạng, nén/resize ảnh, bucket đích, mức nhạy cảm");
            tb.HasCheckConstraint("upload_policies_sensitivity_check", "sensitivity IN ('PUBLIC','INTERNAL','CONFIDENTIAL','RESTRICTED')");
            tb.HasCheckConstraint("upload_policies_image_quality_check", "image_quality BETWEEN 1 AND 100");
            tb.HasCheckConstraint("upload_policies_convert_to_format_check", "convert_to_format IN ('WEBP','JPEG','PNG','AVIF')");
        });

        builder.HasKey(x => x.Id).HasName("upload_policies_pkey");

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
            .HasComment("Tenant (NULL = mặc định)");
        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại file (AVATAR, LABOR_CONTRACT, PAYSLIP, INVOICE, PRODUCT_IMAGE, ATTACHMENT, KB_DOCUMENT)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ sử dụng");
        builder.Property(x => x.BucketId)
            .HasColumnName("bucket_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Bucket lưu");
        builder.Property(x => x.PathTemplate)
            .HasColumnName("path_template")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'{tenant}/{module}/{yyyy}/{mm}/{uuid}{ext}'")
            .ValueGeneratedNever()
            .HasComment("Mẫu object key");
        builder.Property(x => x.MaxSizeMb)
            .HasColumnName("max_size_mb")
            .HasColumnType("numeric(10,2)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("10")
            .ValueGeneratedNever()
            .HasComment("Dung lượng tối đa/file (MB)");
        builder.Property(x => x.AllowedMimeTypes)
            .HasColumnName("allowed_mime_types")
            .HasColumnType("text[]")
            .HasColumnOrder(7)
            .HasComment("MIME cho phép (NULL = tất cả trừ blacklist)");
        builder.Property(x => x.AllowedExtensions)
            .HasColumnName("allowed_extensions")
            .HasColumnType("text[]")
            .HasColumnOrder(8)
            .HasComment("Đuôi file cho phép");
        builder.Property(x => x.Sensitivity)
            .HasColumnName("sensitivity")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'INTERNAL'")
            .ValueGeneratedNever()
            .HasComment("Mức nhạy cảm: RESTRICTED (bảng lương, HĐLĐ) bắt buộc presigned URL ngắn hạn + ghi log truy cập");
        builder.Property(x => x.PresignedTtlSeconds)
            .HasColumnName("presigned_ttl_seconds")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("300")
            .ValueGeneratedNever()
            .HasComment("Thời hạn link tạm (giây)");
        builder.Property(x => x.AutoResize)
            .HasColumnName("auto_resize")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Tự resize ảnh");
        builder.Property(x => x.MaxWidth)
            .HasColumnName("max_width")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .HasComment("Chiều rộng tối đa sau resize (px)");
        builder.Property(x => x.MaxHeight)
            .HasColumnName("max_height")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .HasComment("Chiều cao tối đa (px)");
        builder.Property(x => x.ImageQuality)
            .HasColumnName("image_quality")
            .HasColumnType("smallint")
            .HasColumnOrder(14)
            .HasDefaultValueSql("80")
            .ValueGeneratedNever()
            .HasComment("Chất lượng nén ảnh");
        builder.Property(x => x.ConvertToFormat)
            .HasColumnName("convert_to_format")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(15)
            .HasComment("Chuyển định dạng ảnh");
        builder.Property(x => x.Variants)
            .HasColumnName("variants")
            .HasColumnType("jsonb")
            .HasColumnOrder(16)
            .HasComment("Biến thể cần sinh, VD: [{\"name\":\"THUMB\",\"w\":150,\"h\":150,\"fit\":\"cover\"}]");
        builder.Property(x => x.StripExif)
            .HasColumnName("strip_exif")
            .HasColumnType("boolean")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Xóa EXIF (GPS) khi upload từ mobile");
        builder.Property(x => x.VirusScan)
            .HasColumnName("virus_scan")
            .HasColumnType("boolean")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Quét virus");
        builder.Property(x => x.TempTtlHours)
            .HasColumnName("temp_ttl_hours")
            .HasColumnType("int")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("24")
            .ValueGeneratedNever()
            .HasComment("File tạm (chưa gắn entity) bị dọn sau N giờ");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(25)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(26)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.TenantId, x.Category }, "uq_upload_policies")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("upload_policies_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("upload_policies_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Bucket)
            .WithMany()
            .HasForeignKey(x => x.BucketId)
            .HasConstraintName("upload_policies_bucket_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
