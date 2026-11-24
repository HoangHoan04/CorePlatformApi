// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>signing_keys</c>.
/// </summary>
public sealed class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.ToTable("signing_keys", tb =>
        {
            tb.HasComment("Khóa ký JWT (RS256/ES256) phục vụ JWKS và xoay vòng khóa định kỳ");
            tb.HasCheckConstraint("signing_keys_algorithm_check", "algorithm IN ('RS256','RS384','RS512','ES256','ES384','PS256')");
            tb.HasCheckConstraint("signing_keys_usage_check", "usage IN ('SIGNING','ENCRYPTION')");
            tb.HasCheckConstraint("signing_keys_status_check", "status IN ('PENDING','ACTIVE','RETIRED','REVOKED')");
        });

        builder.HasKey(x => x.Id).HasName("signing_keys_pkey");

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
        builder.Property(x => x.Kid)
            .HasColumnName("kid")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Key ID (header kid trong JWT)");
        builder.Property(x => x.Algorithm)
            .HasColumnName("algorithm")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'RS256'")
            .ValueGeneratedNever()
            .HasComment("Thuật toán ký");
        builder.Property(x => x.Usage)
            .HasColumnName("usage")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'SIGNING'")
            .ValueGeneratedNever()
            .HasComment("Mục đích");
        builder.Property(x => x.PublicKeyPem)
            .HasColumnName("public_key_pem")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Public key (công bố qua /.well-known/jwks.json)");
        builder.Property(x => x.PrivateKeyEncrypted)
            .HasColumnName("private_key_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Private key đã mã hóa");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("PENDING: công bố trước, ACTIVE: đang ký, RETIRED: chỉ còn verify");
        builder.Property(x => x.ActivatedAt)
            .HasColumnName("activated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Bắt đầu ký");
        builder.Property(x => x.RetiredAt)
            .HasColumnName("retired_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Ngừng ký");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Gỡ khỏi JWKS sau thời điểm này");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Kid, "signing_keys_kid_key").IsUnique();
    }
}
