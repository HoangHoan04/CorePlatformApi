// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>refresh_tokens</c>.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", tb =>
        {
            tb.HasComment("Refresh token có rotation: mỗi lần refresh sinh token mới cùng family; dùng lại token cũ → thu hồi cả family (phát hiện đánh cắp)");
        });

        builder.HasKey(x => x.Id).HasName("refresh_tokens_pkey");

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
        builder.Property(x => x.SessionId)
            .HasColumnName("session_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phiên");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("Client được cấp");
        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(128)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("SHA-256 của refresh token (không lưu bản rõ)");
        builder.Property(x => x.FamilyId)
            .HasColumnName("family_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Họ token: chung cho chuỗi rotation từ một lần đăng nhập");
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Token trước đó trong chuỗi rotation");
        builder.Property(x => x.ReplacedById)
            .HasColumnName("replaced_by_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Token mới thay thế (khi đã rotate)");
        builder.Property(x => x.Scopes)
            .HasColumnName("scopes")
            .HasColumnType("text[]")
            .HasColumnOrder(9)
            .HasComment("Scope được cấp");
        builder.Property(x => x.IssuedAt)
            .HasColumnName("issued_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm cấp");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasComment("Hết hạn");
        builder.Property(x => x.UsedAt)
            .HasColumnName("used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Đã dùng để rotate lúc");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thu hồi lúc");
        builder.Property(x => x.RevokedReason)
            .HasColumnName("revoked_reason")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(14)
            .HasComment("Lý do thu hồi");
        builder.Property(x => x.CreatedIp)
            .HasColumnName("created_ip")
            .HasColumnType("inet")
            .HasColumnOrder(15)
            .HasComment("IP yêu cầu cấp");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.TokenHash, "refresh_tokens_token_hash_key").IsUnique();
        builder.HasIndex(x => x.FamilyId, "ix_refresh_tokens_family");
        builder.HasIndex(x => x.SessionId, "ix_refresh_tokens_session")
            .HasFilter("revoked_at IS NULL");
        builder.HasIndex(x => x.ExpiresAt, "ix_refresh_tokens_expires");

        builder.HasOne(x => x.Session)
            .WithMany(p => p.RefreshTokens)
            .HasForeignKey(x => x.SessionId)
            .HasConstraintName("refresh_tokens_session_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany(p => p.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("refresh_tokens_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("refresh_tokens_parent_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
