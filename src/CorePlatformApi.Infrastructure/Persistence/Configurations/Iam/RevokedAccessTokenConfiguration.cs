// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>revoked_access_tokens</c>.
/// </summary>
public sealed class RevokedAccessTokenConfiguration : IEntityTypeConfiguration<RevokedAccessToken>
{
    public void Configure(EntityTypeBuilder<RevokedAccessToken> builder)
    {
        builder.ToTable("revoked_access_tokens", tb =>
        {
            tb.HasComment("Danh sách đen access token (jti) bị thu hồi trước hạn. Bản chính nên ở Redis; bảng này để khôi phục khi Redis mất dữ liệu");
        });

        builder.HasKey(x => x.Id).HasName("revoked_access_tokens_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (UUID v7)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.Jti)
            .HasColumnName("jti")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("JWT ID");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("User");
        builder.Property(x => x.SessionId)
            .HasColumnName("session_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Phiên");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Hết hạn gốc của token (sau thời điểm này xóa bản ghi)");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thu hồi lúc");
        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(7)
            .HasComment("Lý do");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Jti, "revoked_access_tokens_jti_key").IsUnique();
        builder.HasIndex(x => x.ExpiresAt, "ix_revoked_tokens_expires");
    }
}
