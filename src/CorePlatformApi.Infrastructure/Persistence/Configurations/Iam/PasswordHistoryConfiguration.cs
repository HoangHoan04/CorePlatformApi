// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.password_histories</c>.
/// </summary>
public sealed class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("password_histories", "iam", tb =>
        {
            tb.HasComment("Lịch sử mật khẩu để chặn dùng lại N mật khẩu gần nhất");
        });

        builder.HasKey(x => x.Id).HasName("password_histories_pkey");

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
        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Hash mật khẩu cũ");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm đặt mật khẩu này");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt }, "ix_password_histories_user")
            .IsDescending(false, true);

        builder.HasOne(x => x.User)
            .WithMany(p => p.PasswordHistories)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("password_histories_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
