// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.security_policies</c>.
/// </summary>
public sealed class SecurityPolicyConfiguration : IEntityTypeConfiguration<SecurityPolicy>
{
    public void Configure(EntityTypeBuilder<SecurityPolicy> builder)
    {
        builder.ToTable("security_policies", "iam", tb =>
        {
            tb.HasComment("Chính sách bảo mật tài khoản theo tenant (NULL = mặc định toàn hệ thống)");
            tb.HasCheckConstraint("security_policies_device_limit_action_check", "device_limit_action IN ('REVOKE_OLDEST','BLOCK_NEW')");
        });

        builder.HasKey(x => x.Id).HasName("security_policies_pkey");

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
            .HasComment("Tenant (NULL = chính sách mặc định)");
        builder.Property(x => x.PasswordMinLength)
            .HasColumnName("password_min_length")
            .HasColumnType("smallint")
            .HasColumnOrder(2)
            .IsRequired()
            .HasDefaultValueSql("8")
            .ValueGeneratedNever()
            .HasComment("Độ dài tối thiểu");
        builder.Property(x => x.PasswordRequireUpper)
            .HasColumnName("password_require_upper")
            .HasColumnType("boolean")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc chữ hoa");
        builder.Property(x => x.PasswordRequireLower)
            .HasColumnName("password_require_lower")
            .HasColumnType("boolean")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc chữ thường");
        builder.Property(x => x.PasswordRequireDigit)
            .HasColumnName("password_require_digit")
            .HasColumnType("boolean")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc chữ số");
        builder.Property(x => x.PasswordRequireSpecial)
            .HasColumnName("password_require_special")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc ký tự đặc biệt");
        builder.Property(x => x.PasswordExpireDays)
            .HasColumnName("password_expire_days")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Số ngày hết hạn mật khẩu (NULL = không hết hạn)");
        builder.Property(x => x.PasswordHistoryCount)
            .HasColumnName("password_history_count")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("3")
            .ValueGeneratedNever()
            .HasComment("Không được dùng lại N mật khẩu gần nhất");
        builder.Property(x => x.MaxFailedAttempts)
            .HasColumnName("max_failed_attempts")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("5")
            .ValueGeneratedNever()
            .HasComment("Số lần sai tối đa trước khi khóa");
        builder.Property(x => x.LockoutMinutes)
            .HasColumnName("lockout_minutes")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("15")
            .ValueGeneratedNever()
            .HasComment("Thời gian khóa tạm (phút)");
        builder.Property(x => x.ForceChangeOnFirstLogin)
            .HasColumnName("force_change_on_first_login")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Ép đổi mật khẩu lần đầu");
        builder.Property(x => x.RequireMfa)
            .HasColumnName("require_mfa")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc 2FA cho mọi user");
        builder.Property(x => x.RequireMfaForAdmin)
            .HasColumnName("require_mfa_for_admin")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc 2FA cho admin");
        builder.Property(x => x.MaxDevicesPerUser)
            .HasColumnName("max_devices_per_user")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("3")
            .ValueGeneratedNever()
            .HasComment("Số thiết bị đăng nhập đồng thời tối đa");
        builder.Property(x => x.DeviceLimitAction)
            .HasColumnName("device_limit_action")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'REVOKE_OLDEST'")
            .ValueGeneratedNever()
            .HasComment("Khi vượt giới hạn: đá thiết bị cũ nhất hoặc chặn đăng nhập mới");
        builder.Property(x => x.AccessTokenMinutes)
            .HasColumnName("access_token_minutes")
            .HasColumnType("int")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("15")
            .ValueGeneratedNever()
            .HasComment("Thời hạn access token (phút)");
        builder.Property(x => x.RefreshTokenDays)
            .HasColumnName("refresh_token_days")
            .HasColumnType("int")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("30")
            .ValueGeneratedNever()
            .HasComment("Thời hạn refresh token (ngày)");
        builder.Property(x => x.SessionIdleMinutes)
            .HasColumnName("session_idle_minutes")
            .HasColumnType("int")
            .HasColumnOrder(18)
            .HasComment("Tự đăng xuất khi không hoạt động (phút)");
        builder.Property(x => x.AllowedIpRanges)
            .HasColumnName("allowed_ip_ranges")
            .HasColumnType("cidr[]")
            .HasColumnOrder(19)
            .HasComment("Chỉ cho phép đăng nhập từ dải IP này (NULL = không giới hạn)");
        builder.Property(x => x.AllowedLoginHours)
            .HasColumnName("allowed_login_hours")
            .HasColumnType("jsonb")
            .HasColumnOrder(20)
            .HasComment("Khung giờ cho phép đăng nhập theo thứ, VD: {\"mon\":[\"07:00\",\"19:00\"]}");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.TenantId, "security_policies_tenant_id_key").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("security_policies_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
