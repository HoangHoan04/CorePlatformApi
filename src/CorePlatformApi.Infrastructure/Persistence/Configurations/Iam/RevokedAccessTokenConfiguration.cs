// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.revoked_access_tokens</c>.
/// </summary>
public sealed class RevokedAccessTokenConfiguration : IEntityTypeConfiguration<RevokedAccessToken>
{
    public void Configure(EntityTypeBuilder<RevokedAccessToken> builder)
    {
        builder.ToTable("revoked_access_tokens", "iam", tb =>
        {
            tb.HasComment("Danh sách đen access token (jti) bị thu hồi trước hạn. Bản chính nên ở Redis; bảng này để khôi phục khi Redis mất dữ liệu");
        });

        builder.HasKey(x => x.Jti).HasName("revoked_access_tokens_pkey");

        builder.Property(x => x.Jti)
            .HasColumnName("jti")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("JWT ID");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("User");
        builder.Property(x => x.SessionId)
            .HasColumnName("session_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Phiên");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Hết hạn gốc của token (sau thời điểm này xóa bản ghi)");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thu hồi lúc");
        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(5)
            .HasComment("Lý do");

        builder.HasIndex(x => x.ExpiresAt, "ix_revoked_tokens_expires");
    }
}
