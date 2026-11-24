// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho bảng <c>storage_providers</c>.
/// </summary>
public sealed class StorageProviderConfiguration : IEntityTypeConfiguration<StorageProvider>
{
    public void Configure(EntityTypeBuilder<StorageProvider> builder)
    {
        builder.ToTable("storage_providers", tb =>
        {
            tb.HasComment("Cấu hình nhà cung cấp Object Storage (MinIO, S3, R2...). Hỗ trợ tenant dùng storage riêng");
            tb.HasCheckConstraint("storage_providers_provider_type_check", "provider_type IN ('MINIO','S3','R2','GCS','AZURE_BLOB','LOCAL')");
        });

        builder.HasKey(x => x.Id).HasName("storage_providers_pkey");

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
            .HasComment("Tenant (NULL = storage hệ thống)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã cấu hình");
        builder.Property(x => x.ProviderType)
            .HasColumnName("provider_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Loại provider");
        builder.Property(x => x.Endpoint)
            .HasColumnName("endpoint")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(4)
            .HasComment("Endpoint (https://minio.internal:9000)");
        builder.Property(x => x.PublicBaseUrl)
            .HasColumnName("public_base_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .HasComment("URL công khai/CDN cho file public");
        builder.Property(x => x.Region)
            .HasColumnName("region")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(6)
            .HasComment("Region");
        builder.Property(x => x.AccessKeyEncrypted)
            .HasColumnName("access_key_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Access key đã mã hóa");
        builder.Property(x => x.SecretKeyEncrypted)
            .HasColumnName("secret_key_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(8)
            .HasComment("Secret key đã mã hóa");
        builder.Property(x => x.UsePathStyle)
            .HasColumnName("use_path_style")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Path-style URL (MinIO cần true)");
        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Provider mặc định");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.TenantId, x.Code }, "uq_storage_providers_code")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("storage_providers_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
