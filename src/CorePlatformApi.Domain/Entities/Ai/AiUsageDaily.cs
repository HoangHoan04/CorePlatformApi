// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Tổng hợp usage theo ngày (job cộng dồn từ usage_logs) — dùng kiểm tra quota nhanh và báo cáo chi phí
/// </summary>
/// <remarks>Bảng <c>usage_daily</c> (phân hệ ai).</remarks>
public class AiUsageDaily : BaseEntity
{
    /// <summary>
    /// Ngày
    /// </summary>
    public DateOnly UsageDate { get; set; }

    /// <summary>
    /// Phân hệ ('-' nếu không xác định)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'-'</c></remarks>
    public string ModuleCode { get; set; } = "-";

    /// <summary>
    /// Model
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Số request
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int RequestCount { get; set; } = 0;

    /// <summary>
    /// Số request lỗi
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int ErrorCount { get; set; } = 0;

    /// <summary>
    /// Tổng token vào
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public long InputTokens { get; set; } = 0;

    /// <summary>
    /// Tổng token ra
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public long OutputTokens { get; set; } = 0;

    /// <summary>
    /// Tổng chi phí
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public decimal CostUsd { get; set; } = 0m;
}
