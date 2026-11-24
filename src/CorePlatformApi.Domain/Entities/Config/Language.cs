// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Ngôn ngữ hệ thống hỗ trợ
/// </summary>
/// <remarks>Bảng <c>languages</c> (phân hệ config).</remarks>
public class Language : BaseEntity
{
    /// <summary>
    /// Mã BCP-47 (vi-VN, en-US)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên tiếng Việt
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tên bản ngữ (Tiếng Việt, English)
    /// </summary>
    public string NativeName { get; set; } = string.Empty;

    /// <summary>
    /// Hướng chữ
    /// </summary>
    /// <remarks>Ràng buộc: <c>direction IN ('LTR','RTL')</c>; Mặc định DB: <c>'LTR'</c></remarks>
    public string Direction { get; set; } = "LTR";

    /// <summary>
    /// Định dạng ngày
    /// </summary>
    /// <remarks>Mặc định DB: <c>'dd/MM/yyyy'</c></remarks>
    public string DateFormat { get; set; } = "dd/MM/yyyy";

    /// <summary>
    /// Dấu thập phân, nhóm nghìn
    /// </summary>
    public string? NumberFormat { get; set; }

    /// <summary>
    /// Ngôn ngữ mặc định
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Đang hỗ trợ
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thứ tự
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;
}
