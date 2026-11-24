// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Rule target của flag theo tenant/công ty/chi nhánh/vai trò/user, có thời gian hiệu lực
/// </summary>
/// <remarks>Bảng <c>feature_flag_rules</c> (phân hệ config).</remarks>
public class FeatureFlagRule : BaseEntity
{
    /// <summary>
    /// Flag
    /// </summary>
    /// <remarks>FK → <c>config.feature_flags(key)</c></remarks>
    public string FlagKey { get; set; } = string.Empty;

    /// <summary>
    /// Đối tượng áp dụng
    /// </summary>
    /// <remarks>Ràng buộc: <c>scope_type IN ('TENANT','COMPANY','BRANCH','ROLE','USER','PLAN','ENV')</c></remarks>
    public string ScopeType { get; set; } = string.Empty;

    /// <summary>
    /// Id hoặc mã đối tượng (uuid, mã gói, production/staging)
    /// </summary>
    public string ScopeValue { get; set; } = string.Empty;

    /// <summary>
    /// Bật/tắt cho đối tượng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// % rollout (PERCENTAGE)
    /// </summary>
    /// <remarks>Ràng buộc: <c>percentage BETWEEN 0 AND 100</c></remarks>
    public short? Percentage { get; set; }

    /// <summary>
    /// Biến thể trả về (VARIANT)
    /// </summary>
    public string? Variant { get; set; }

    /// <summary>
    /// Ưu tiên (số lớn xét trước)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Bắt đầu hiệu lực
    /// </summary>
    public DateTimeOffset? StartAt { get; set; }

    /// <summary>
    /// Kết thúc hiệu lực
    /// </summary>
    public DateTimeOffset? EndAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Feature flag bật/tắt tính năng không cần deploy (qua <c>flag_key</c>).
    /// </summary>
    public FeatureFlag Flag { get; set; } = null!;

    #endregion
}
