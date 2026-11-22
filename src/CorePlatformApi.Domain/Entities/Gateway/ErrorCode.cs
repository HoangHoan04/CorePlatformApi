// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Mã lỗi chuẩn hóa toàn hệ sinh thái. Response lỗi: {success:false, code, message, traceId, errors[]}
/// </summary>
/// <remarks>Bảng <c>gateway.error_codes</c>.</remarks>
public class ErrorCode : AuditableEntity
{
    /// <summary>
    /// Mã lỗi (AUTH_TOKEN_EXPIRED, HRM_LEAVE_BALANCE_INSUFFICIENT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// HTTP status tương ứng
    /// </summary>
    /// <remarks>Ràng buộc: <c>http_status BETWEEN 400 AND 599</c></remarks>
    public short HttpStatus { get; set; }

    /// <summary>
    /// Nhóm lỗi
    /// </summary>
    /// <remarks>Ràng buộc: <c>category IN ('VALIDATION','AUTHENTICATION','AUTHORIZATION','NOT_FOUND','CONFLICT','BUSINESS','RATE_LIMIT','INTEGRATION','SYSTEM')</c></remarks>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Khóa i18n của thông báo (config.translation_keys)
    /// </summary>
    public string? MessageKey { get; set; }

    /// <summary>
    /// Thông báo mặc định tiếng Việt
    /// </summary>
    public string DefaultMessage { get; set; } = string.Empty;

    /// <summary>
    /// Giải thích cho dev / hướng xử lý
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Client có nên thử lại
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsRetryable { get; set; } = false;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    #endregion
}
