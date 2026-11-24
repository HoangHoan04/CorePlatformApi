// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Bộ đếm hiện tại của từng quy tắc theo kỳ (năm/tháng/ngày) và phạm vi (công ty/chi nhánh)
/// </summary>
/// <remarks>Bảng <c>numbering_counters</c> (phân hệ mdm).</remarks>
public class NumberingCounter : BaseEntity
{
    /// <summary>
    /// Quy tắc
    /// </summary>
    /// <remarks>FK → <c>mdm.numbering_rules(id)</c></remarks>
    public Guid RuleId { get; set; }

    /// <summary>
    /// Khóa kỳ: 'ALL', '2026', '202611', '20261121'
    /// </summary>
    public string PeriodKey { get; set; } = string.Empty;

    /// <summary>
    /// Khóa phạm vi: '-' hoặc id công ty/chi nhánh
    /// </summary>
    /// <remarks>Mặc định DB: <c>'-'</c></remarks>
    public string ScopeKey { get; set; } = "-";

    /// <summary>
    /// Giá trị đã cấp gần nhất
    /// </summary>
    public long CurrentValue { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Quy tắc sinh mã tự động theo loại chứng từ, có thể riêng theo công ty/chi nhánh (qua <c>rule_id</c>).
    /// </summary>
    public NumberingRule Rule { get; set; } = null!;

    #endregion
}
