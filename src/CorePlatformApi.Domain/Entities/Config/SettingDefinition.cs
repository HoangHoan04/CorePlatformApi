// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Định nghĩa khóa cấu hình (kiểu dữ liệu, mặc định, phạm vi cho phép ghi đè) — thay cho hardcode
/// </summary>
/// <remarks>Bảng <c>setting_definitions</c> (phân hệ config).</remarks>
public class SettingDefinition : BaseEntity
{
    /// <summary>
    /// Khóa cấu hình (hrm.attendance.late_grace_minutes)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Nhóm hiển thị trên màn hình cài đặt
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Giải thích
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Kiểu dữ liệu
    /// </summary>
    /// <remarks>Ràng buộc: <c>data_type IN ('STRING','INT','DECIMAL','BOOL','DATE','TIME','JSON','ENUM','SECRET')</c></remarks>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị mặc định
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Danh sách lựa chọn (ENUM)
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// Ràng buộc (min, max, regex)
    /// </summary>
    public string? Validation { get; set; }

    /// <summary>
    /// Phạm vi được ghi đè: SYSTEM, TENANT, COMPANY, BRANCH, USER
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{SYSTEM,TENANT}'</c></remarks>
    public List<string> AllowedScopes { get; set; } = ["SYSTEM", "TENANT"];

    /// <summary>
    /// Trả về cho frontend (không bí mật)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Cần restart service để có hiệu lực
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool RequiresRestart { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Danh sách SettingValue con (qua <c>config.setting_values.setting_key</c>).
    /// </summary>
    public ICollection<SettingValue> SettingValues { get; set; } = [];

    #endregion
}
