// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho <c>storage.file_variants</c>.
/// </summary>
public sealed class FileVariantConfiguration : IEntityTypeConfiguration<FileVariant>
{
    public void Configure(EntityTypeBuilder<FileVariant> builder)
    {
        builder.ToTable("file_variants", "storage", tb =>
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
        builder.Property(x => x.FileId)
            .HasColumnName("file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("File gốc");
        builder.Property(x => x.VariantName)
            .HasColumnName("variant_name")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên biến thể (THUMB, SMALL, MEDIUM, LARGE)");
        builder.Property(x => x.ObjectKey)
            .HasColumnName("object_key")
            .HasColumnType("varchar(1000)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Object key biến thể");
        builder.Property(x => x.MimeType)
            .HasColumnName("mime_type")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("MIME type");
        builder.Property(x => x.SizeBytes)
            .HasColumnName("size_bytes")
            .HasColumnType("bigint")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Dung lượng");
        builder.Property(x => x.Width)
            .HasColumnName("width")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .HasComment("Rộng (px)");
        builder.Property(x => x.Height)
            .HasColumnName("height")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Cao (px)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");

        builder.HasIndex(x => new { x.FileId, x.VariantName }, "uq_file_variants").IsUnique();

        builder.HasOne(x => x.File)
            .WithMany(p => p.FileVariants)
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("file_variants_file_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
