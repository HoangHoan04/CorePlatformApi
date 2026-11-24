// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Danh sách model đích của alias theo thứ tự ưu tiên (model chính → fallback)
/// </summary>
/// <remarks>Bảng <c>model_alias_targets</c> (phân hệ ai).</remarks>
public class AiModelAliasTarget : BaseEntity
{
    /// <summary>
    /// Alias
    /// </summary>
    /// <remarks>FK → <c>ai.model_aliases(id)</c></remarks>
    public Guid AliasId { get; set; }

    /// <summary>
    /// Model
    /// </summary>
    /// <remarks>FK → <c>ai.models(id)</c></remarks>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Ép dùng key này (NULL = tự chọn)
    /// </summary>
    /// <remarks>FK → <c>ai.provider_credentials(id)</c></remarks>
    public Guid? CredentialId { get; set; }

    /// <summary>
    /// 1 = chính, 2,3 = fallback khi lỗi/429
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short Priority { get; set; } = 1;

    /// <summary>
    /// Trọng số chia tải khi cùng priority
    /// </summary>
    /// <remarks>Mặc định DB: <c>100</c></remarks>
    public short Weight { get; set; } = 100;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Alias model logic (default-chat, cheap-summary, embedding-default) để đổi model không cần sửa code; hỗ trợ fallback (qua <c>alias_id</c>).
    /// </summary>
    public AiModelAlias Alias { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Model AI và bảng giá để ước tính chi phí (qua <c>model_id</c>).
    /// </summary>
    public AiModel Model { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới API key của nhà cung cấp AI — lưu tập trung, các service con không giữ key (qua <c>credential_id</c>).
    /// </summary>
    public AiProviderCredential? Credential { get; set; }

    #endregion
}
