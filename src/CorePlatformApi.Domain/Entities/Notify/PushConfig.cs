// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Cấu hình nhà cung cấp push theo ứng dụng mobile (FCM, APNs)
/// </summary>
/// <remarks>Bảng <c>push_configs</c> (phân hệ notify).</remarks>
public class PushConfig : BaseEntity
{
    /// <summary>
    /// Mã app (HRM_MOBILE, WMS_MOBILE)
    /// </summary>
    public string AppCode { get; set; } = string.Empty;

    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    /// <remarks>Ràng buộc: <c>provider IN ('FCM','APNS','HMS')</c></remarks>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Firebase project id
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// iOS bundle id / Android package name
    /// </summary>
    public string? BundleId { get; set; }

    /// <summary>
    /// Service account JSON (FCM) hoặc .p8 key (APNs) đã mã hóa
    /// </summary>
    public string CredentialsEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// APNs Key ID
    /// </summary>
    public string? ApnsKeyId { get; set; }

    /// <summary>
    /// Apple Team ID
    /// </summary>
    public string? ApnsTeamId { get; set; }

    /// <summary>
    /// Môi trường production/sandbox
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsProduction { get; set; } = true;

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

    #endregion
}
