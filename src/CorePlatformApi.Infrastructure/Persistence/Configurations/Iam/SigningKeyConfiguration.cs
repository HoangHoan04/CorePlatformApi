// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.signing_keys</c>.
/// </summary>
public sealed class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.ToTable("signing_keys", "iam", tb =>
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
        builder.Property(x => x.Kid)
            .HasColumnName("kid")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Key ID (header kid trong JWT)");
        builder.Property(x => x.Algorithm)
            .HasColumnName("algorithm")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasDefaultValueSql("'RS256'")
            .ValueGeneratedNever()
            .HasComment("Thuật toán ký");
        builder.Property(x => x.Usage)
            .HasColumnName("usage")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'SIGNING'")
            .ValueGeneratedNever()
            .HasComment("Mục đích");
        builder.Property(x => x.PublicKeyPem)
            .HasColumnName("public_key_pem")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Public key (công bố qua /.well-known/jwks.json)");
        builder.Property(x => x.PrivateKeyEncrypted)
            .HasColumnName("private_key_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Private key đã mã hóa");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("PENDING: công bố trước, ACTIVE: đang ký, RETIRED: chỉ còn verify");
        builder.Property(x => x.ActivatedAt)
            .HasColumnName("activated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Bắt đầu ký");
        builder.Property(x => x.RetiredAt)
            .HasColumnName("retired_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Ngừng ký");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Gỡ khỏi JWKS sau thời điểm này");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");

        builder.HasIndex(x => x.Kid, "signing_keys_kid_key").IsUnique();
    }
}
