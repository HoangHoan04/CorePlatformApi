// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Chính sách rate limit/throttling theo IP, user, tenant, client, API key
/// </summary>
/// <remarks>Bảng <c>rate_limit_policies</c> (phân hệ gateway).</remarks>
public class RateLimitPolicy : BaseEntity
{
    /// <summary>
    /// Mã chính sách (default-user, login-strict, partner-basic)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Đếm theo khóa nào
    /// </summary>
    /// <remarks>Ràng buộc: <c>partition_by IN ('IP','USER','TENANT','CLIENT','API_KEY','GLOBAL')</c></remarks>
    public string PartitionBy { get; set; } = string.Empty;

    /// <summary>
    /// Thuật toán
    /// </summary>
    /// <remarks>Ràng buộc: <c>algorithm IN ('FIXED_WINDOW','SLIDING_WINDOW','TOKEN_BUCKET','CONCURRENCY')</c>; Mặc định DB: <c>'SLIDING_WINDOW'</c></remarks>
    public string Algorithm { get; set; } = "SLIDING_WINDOW";

    /// <summary>
    /// Số request cho phép trong cửa sổ
    /// </summary>
    /// <remarks>Ràng buộc: <c>permit_limit &gt; 0</c></remarks>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Độ dài cửa sổ (giây)
    /// </summary>
    /// <remarks>Mặc định DB: <c>60</c></remarks>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Số phân đoạn (SLIDING_WINDOW)
    /// </summary>
    public short? Segments { get; set; }

    /// <summary>
    /// Token nạp lại mỗi chu kỳ (TOKEN_BUCKET)
    /// </summary>
    public int? TokensPerPeriod { get; set; }

    /// <summary>
    /// Số request được xếp hàng khi vượt
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int QueueLimit { get; set; } = 0;

    /// <summary>
    /// HTTP status khi từ chối
    /// </summary>
    /// <remarks>Mặc định DB: <c>429</c></remarks>
    public short RejectStatus { get; set; } = 429;

    /// <summary>
    /// Đang dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Danh sách TenantRateLimit con (qua <c>gateway.tenant_rate_limits.policy_id</c>).
    /// </summary>
    public ICollection<TenantRateLimit> TenantRateLimits { get; set; } = [];

    #endregion
}
