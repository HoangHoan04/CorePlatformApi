// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Giá trị cấu hình theo phạm vi. Thứ tự ưu tiên khi đọc: USER &gt; BRANCH &gt; COMPANY &gt; TENANT &gt; SYSTEM &gt; default_value
/// </summary>
/// <remarks>Bảng <c>setting_values</c> (phân hệ config).</remarks>
public class SettingValue : BaseEntity
{
    /// <summary>
    /// Khóa cấu hình
    /// </summary>
    /// <remarks>FK → <c>config.setting_definitions(key)</c></remarks>
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// Phạm vi
    /// </summary>
    /// <remarks>Ràng buộc: <c>scope_type IN ('SYSTEM','TENANT','COMPANY','BRANCH','USER')</c></remarks>
    public string ScopeType { get; set; } = string.Empty;

    /// <summary>
    /// Id tenant/công ty/chi nhánh/user (NULL khi SYSTEM)
    /// </summary>
    public Guid? ScopeId { get; set; }

    /// <summary>
    /// Giá trị
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Giá trị đã mã hóa (kiểu SECRET)
    /// </summary>
    public string? ValueEncrypted { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Định nghĩa khóa cấu hình (kiểu dữ liệu, mặc định, phạm vi cho phép ghi đè) — thay cho hardcode (qua <c>setting_key</c>).
    /// </summary>
    public SettingDefinition Setting { get; set; } = null!;

    #endregion
}
