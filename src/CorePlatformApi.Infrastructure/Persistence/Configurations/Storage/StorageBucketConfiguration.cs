// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Storage;

/// <summary>
/// Cấu hình EF Core cho bảng <c>buckets</c>.
/// </summary>
public sealed class StorageBucketConfiguration : IEntityTypeConfiguration<StorageBucket>
{
    public void Configure(EntityTypeBuilder<StorageBucket> builder)
    {
        builder.ToTable("buckets", tb =>
        {
            tb.HasComment("Bucket lưu trữ theo mục đích (public-assets, private-docs, hr-sensitive...)");
            tb.HasCheckConstraint("buckets_encryption_check", "encryption IN ('NONE','SSE','SSE_KMS')");
        });

        builder.HasKey(x => x.Id).HasName("buckets_pkey");

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
        builder.Property(x => x.ProviderId)
            .HasColumnName("provider_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Provider");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên bucket thật trên storage");
        builder.Property(x => x.Purpose)
            .HasColumnName("purpose")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("Mục đích sử dụng");
        builder.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .HasColumnType("boolean")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho phép đọc công khai");
        builder.Property(x => x.Versioning)
            .HasColumnName("versioning")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bật versioning trên storage");
        builder.Property(x => x.Encryption)
            .HasColumnName("encryption")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'SSE'")
            .ValueGeneratedNever()
            .HasComment("Mã hóa phía server");
        builder.Property(x => x.RetentionDays)
            .HasColumnName("retention_days")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .HasComment("Tự xóa file sau N ngày (lifecycle)");
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
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
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

        builder.HasIndex(x => new { x.ProviderId, x.Name }, "uq_buckets").IsUnique();

        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .HasConstraintName("buckets_provider_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
