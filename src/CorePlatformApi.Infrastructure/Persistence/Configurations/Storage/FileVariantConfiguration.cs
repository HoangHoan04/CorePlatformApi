// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho bảng <c>file_variants</c>.
/// </summary>
public sealed class FileVariantConfiguration : IEntityTypeConfiguration<FileVariant>
{
    public void Configure(EntityTypeBuilder<FileVariant> builder)
    {
        builder.ToTable("file_variants", tb =>
        {
            tb.HasComment("Biến thể của file ảnh sau xử lý (thumbnail, small, medium, webp)");
        });

        builder.HasKey(x => x.Id).HasName("file_variants_pkey");

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
            .HasComment("File gốc");
        builder.Property(x => x.VariantName)
            .HasColumnName("variant_name")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên biến thể (THUMB, SMALL, MEDIUM, LARGE)");
        builder.Property(x => x.ObjectKey)
            .HasColumnName("object_key")
            .HasColumnType("varchar(1000)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Object key biến thể");
        builder.Property(x => x.MimeType)
            .HasColumnName("mime_type")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("MIME type");
        builder.Property(x => x.SizeBytes)
            .HasColumnName("size_bytes")
            .HasColumnType("bigint")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Dung lượng");
        builder.Property(x => x.Width)
            .HasColumnName("width")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Rộng (px)");
        builder.Property(x => x.Height)
            .HasColumnName("height")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .HasComment("Cao (px)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");
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

        builder.HasIndex(x => new { x.FileId, x.VariantName }, "uq_file_variants").IsUnique();

        builder.HasOne(x => x.File)
            .WithMany(p => p.FileVariants)
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("file_variants_file_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
