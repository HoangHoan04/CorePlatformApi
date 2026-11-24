// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Cấu hình SMTP theo hệ thống, tenant hoặc từng công ty
/// </summary>
/// <remarks>Bảng <c>smtp_configs</c> (phân hệ notify).</remarks>
public class SmtpConfig : BaseEntity
{
    /// <summary>
    /// Công ty dùng SMTP riêng
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Tên cấu hình
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cách gửi
    /// </summary>
    /// <remarks>Ràng buộc: <c>provider IN ('SMTP','SES','SENDGRID','MAILGUN','GMAIL_API','M365')</c>; Mặc định DB: <c>'SMTP'</c></remarks>
    public string Provider { get; set; } = "SMTP";

    /// <summary>
    /// SMTP host
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// SMTP port (587, 465)
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Mã hóa kết nối
    /// </summary>
    /// <remarks>Ràng buộc: <c>security IN ('NONE','SSL','STARTTLS')</c>; Mặc định DB: <c>'STARTTLS'</c></remarks>
    public string Security { get; set; } = "STARTTLS";

    /// <summary>
    /// Tài khoản
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Mật khẩu/API key đã mã hóa
    /// </summary>
    public string? PasswordEncrypted { get; set; }

    /// <summary>
    /// Email người gửi
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Tên người gửi
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Reply-To
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Giới hạn số email/phút
    /// </summary>
    public int? RateLimitPerMinute { get; set; }

    /// <summary>
    /// Mặc định trong phạm vi
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Lần test gửi thành công
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// Đang dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    #endregion
}
