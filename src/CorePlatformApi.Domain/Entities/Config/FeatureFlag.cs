// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Feature flag bật/tắt tính năng không cần deploy
/// </summary>
/// <remarks>Bảng <c>feature_flags</c> (phân hệ config).</remarks>
public class FeatureFlag : BaseEntity
{
    /// <summary>
    /// Khóa flag (wms.pos.offline_mode)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bật/tắt, theo % rollout, theo biến thể
    /// </summary>
    /// <remarks>Ràng buộc: <c>flag_type IN ('BOOLEAN','PERCENTAGE','VARIANT')</c>; Mặc định DB: <c>'BOOLEAN'</c></remarks>
    public string FlagType { get; set; } = "BOOLEAN";

    /// <summary>
    /// Mặc định khi không có rule khớp
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool DefaultEnabled { get; set; } = false;

    /// <summary>
    /// Danh sách biến thể (VARIANT)
    /// </summary>
    public string? Variants { get; set; }

    /// <summary>
    /// Công tắc khẩn cấp tắt tính năng toàn hệ thống
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsKillSwitch { get; set; } = false;

    /// <summary>
    /// Người/team phụ trách
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Hạn dọn flag khỏi code
    /// </summary>
    public DateOnly? RemoveAfter { get; set; }

    /// <summary>
    /// Flag còn được đánh giá
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Danh sách FeatureFlagRule con (qua <c>config.feature_flag_rules.flag_key</c>).
    /// </summary>
    public ICollection<FeatureFlagRule> FeatureFlagRules { get; set; } = [];

    #endregion
}
