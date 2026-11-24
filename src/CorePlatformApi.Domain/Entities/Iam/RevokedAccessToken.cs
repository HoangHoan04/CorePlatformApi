// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Danh sách đen access token (jti) bị thu hồi trước hạn. Bản chính nên ở Redis; bảng này để khôi phục khi Redis mất dữ liệu
/// </summary>
/// <remarks>Bảng <c>revoked_access_tokens</c> (phân hệ iam).</remarks>
public class RevokedAccessToken : BaseEntity
{
    /// <summary>
    /// JWT ID
    /// </summary>
    public string Jti { get; set; } = string.Empty;

    /// <summary>
    /// User
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Phiên
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Hết hạn gốc của token (sau thời điểm này xóa bản ghi)
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Thu hồi lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset RevokedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Lý do
    /// </summary>
    public string? Reason { get; set; }
}
