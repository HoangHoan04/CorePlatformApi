// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.user_recovery_codes</c>.
/// </summary>
public sealed class UserRecoveryCodeConfiguration : IEntityTypeConfiguration<UserRecoveryCode>
{
    public void Configure(EntityTypeBuilder<UserRecoveryCode> builder)
    {
        builder.ToTable("user_recovery_codes", "iam", tb =>
        {
            tb.HasComment("Mã khôi phục 2FA dùng một lần");
        });

        builder.HasKey(x => x.Id).HasName("user_recovery_codes_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.CodeHash)
            .HasColumnName("code_hash")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Hash mã khôi phục");
        builder.Property(x => x.UsedAt)
            .HasColumnName("used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(3)
            .HasComment("Thời điểm đã dùng (NULL = chưa dùng)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");

        builder.HasIndex(x => x.UserId, "ix_recovery_codes_user")
            .HasFilter("used_at IS NULL");

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserRecoveryCodes)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_recovery_codes_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
