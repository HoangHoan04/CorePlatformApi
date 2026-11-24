// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Alias model logic (default-chat, cheap-summary, embedding-default) để đổi model không cần sửa code; hỗ trợ fallback
/// </summary>
/// <remarks>Bảng <c>model_aliases</c> (phân hệ ai).</remarks>
public class AiModelAlias : BaseEntity
{
    /// <summary>
    /// Mã alias
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả mục đích
    /// </summary>
    public string? Description { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Danh sách AiModelAliasTarget con (qua <c>ai.model_alias_targets.alias_id</c>).
    /// </summary>
    public ICollection<AiModelAliasTarget> AiModelAliasTargets { get; set; } = [];

    #endregion
}
