// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>verification_tokens</c>.
/// </summary>
public sealed class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("verification_tokens", tb =>
        {
            tb.HasComment("Token một lần: quên mật khẩu, xác thực email/SĐT, lời mời kích hoạt, OTP đăng nhập");
            tb.HasCheckConstraint("verification_tokens_purpose_check", "purpose IN ('RESET_PASSWORD','VERIFY_EMAIL','VERIFY_PHONE','INVITATION','LOGIN_OTP','CHANGE_EMAIL')");
        });

        builder.HasKey(x => x.Id).HasName("verification_tokens_pkey");

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
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.Purpose)
            .HasColumnName("purpose")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mục đích");
        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Hash của token/OTP (không lưu bản rõ)");
        builder.Property(x => x.Target)
            .HasColumnName("target")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .HasComment("Email/SĐT nhận token (khi đổi email)");
        builder.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần nhập sai OTP");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Hết hạn");
        builder.Property(x => x.UsedAt)
            .HasColumnName("used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Đã dùng lúc");
        builder.Property(x => x.CreatedIp)
            .HasColumnName("created_ip")
            .HasColumnType("inet")
            .HasColumnOrder(9)
            .HasComment("IP yêu cầu");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.TokenHash, "ix_verification_tokens_lookup")
            .HasFilter("used_at IS NULL");
        builder.HasIndex(x => new { x.UserId, x.Purpose }, "ix_verification_tokens_user");

        builder.HasOne(x => x.User)
            .WithMany(p => p.VerificationTokens)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("verification_tokens_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
