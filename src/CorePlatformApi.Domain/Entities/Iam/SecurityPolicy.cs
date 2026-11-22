// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Chính sách bảo mật tài khoản theo tenant (NULL = mặc định toàn hệ thống)
/// </summary>
/// <remarks>Bảng <c>iam.security_policies</c>.</remarks>
public class SecurityPolicy : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = chính sách mặc định)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Độ dài tối thiểu
    /// </summary>
    /// <remarks>Mặc định DB: <c>8</c></remarks>
    public short PasswordMinLength { get; set; } = 8;

    /// <summary>
    /// Bắt buộc chữ hoa
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool PasswordRequireUpper { get; set; } = true;

    /// <summary>
    /// Bắt buộc chữ thường
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool PasswordRequireLower { get; set; } = true;

    /// <summary>
    /// Bắt buộc chữ số
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool PasswordRequireDigit { get; set; } = true;

    /// <summary>
    /// Bắt buộc ký tự đặc biệt
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool PasswordRequireSpecial { get; set; } = false;

    /// <summary>
    /// Số ngày hết hạn mật khẩu (NULL = không hết hạn)
    /// </summary>
    public int? PasswordExpireDays { get; set; }

    /// <summary>
    /// Không được dùng lại N mật khẩu gần nhất
    /// </summary>
    /// <remarks>Mặc định DB: <c>3</c></remarks>
    public short PasswordHistoryCount { get; set; } = 3;

    /// <summary>
    /// Số lần sai tối đa trước khi khóa
    /// </summary>
    /// <remarks>Mặc định DB: <c>5</c></remarks>
    public short MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Thời gian khóa tạm (phút)
    /// </summary>
    /// <remarks>Mặc định DB: <c>15</c></remarks>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>
    /// Ép đổi mật khẩu lần đầu
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool ForceChangeOnFirstLogin { get; set; } = true;

    /// <summary>
    /// Bắt buộc 2FA cho mọi user
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool RequireMfa { get; set; } = false;

    /// <summary>
    /// Bắt buộc 2FA cho admin
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool RequireMfaForAdmin { get; set; } = true;

    /// <summary>
    /// Số thiết bị đăng nhập đồng thời tối đa
    /// </summary>
    /// <remarks>Mặc định DB: <c>3</c></remarks>
    public int MaxDevicesPerUser { get; set; } = 3;

    /// <summary>
    /// Khi vượt giới hạn: đá thiết bị cũ nhất hoặc chặn đăng nhập mới
    /// </summary>
    /// <remarks>Ràng buộc: <c>device_limit_action IN ('REVOKE_OLDEST','BLOCK_NEW')</c>; Mặc định DB: <c>'REVOKE_OLDEST'</c></remarks>
    public string DeviceLimitAction { get; set; } = "REVOKE_OLDEST";

    /// <summary>
    /// Thời hạn access token (phút)
    /// </summary>
    /// <remarks>Mặc định DB: <c>15</c></remarks>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>
    /// Thời hạn refresh token (ngày)
    /// </summary>
    /// <remarks>Mặc định DB: <c>30</c></remarks>
    public int RefreshTokenDays { get; set; } = 30;

    /// <summary>
    /// Tự đăng xuất khi không hoạt động (phút)
    /// </summary>
    public int? SessionIdleMinutes { get; set; }

    /// <summary>
    /// Chỉ cho phép đăng nhập từ dải IP này (NULL = không giới hạn)
    /// </summary>
    public List<IPNetwork>? AllowedIpRanges { get; set; }

    /// <summary>
    /// Khung giờ cho phép đăng nhập theo thứ, VD: {"mon":["07:00","19:00"]}
    /// </summary>
    public string? AllowedLoginHours { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}
