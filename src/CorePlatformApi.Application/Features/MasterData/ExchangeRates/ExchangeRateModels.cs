using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.MasterData.ExchangeRates;

/// <summary>Loại tỷ giá (khớp CHECK rate_type). Tên hiển thị: <c>enums.exchangeRateType.{MÃ}</c>.</summary>
public static class ExchangeRateTypes
{
    public static readonly IReadOnlyList<string> All = ["BUY_CASH", "BUY_TRANSFER", "SELL", "TRANSFER", "CENTRAL", "ACCOUNTING"];
    public const string Default = "TRANSFER";
}

public sealed class ExchangeRateQuery : PagedRequest
{
    public string? FromCurrency { get; set; }
    public string? ToCurrency { get; set; }
    public string? RateType { get; set; }
    public string? Source { get; set; }

    /// <summary>Lọc theo ngày áp dụng (theo ngày, tính cả ngày cuối).</summary>
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public sealed record ExchangeRateDto(
    Guid Id,
    Guid? TenantId,
    bool IsShared,
    string FromCurrency,
    string ToCurrency,
    string RateType,
    decimal Rate,
    DateTimeOffset EffectiveAt,
    string Source,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed class ExchangeRateSaveRequest
{
    public Guid? Id { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public string RateType { get; set; } = ExchangeRateTypes.Default;
    public decimal Rate { get; set; }
    public DateTimeOffset? EffectiveAt { get; set; }
    public string? Source { get; set; }
    public string? Note { get; set; }
}

/// <summary>Tra tỷ giá hiệu lực tại một thời điểm (GET latest).</summary>
public sealed class ExchangeRateLookupQuery
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string? RateType { get; set; }

    /// <summary>Thời điểm cần tỷ giá; bỏ trống = hiện tại.</summary>
    public DateTimeOffset? At { get; set; }
}

/// <summary>
/// Kết quả tra tỷ giá: 1 From = Rate To. IsInverse = true nếu chỉ có tỷ giá chiều ngược lại (Rate = 1 / tỷ giá gốc).
/// </summary>
public sealed record ExchangeRateLookupDto(
    string From,
    string To,
    string RateType,
    decimal Rate,
    DateTimeOffset EffectiveAt,
    string Source,
    bool IsInverse,
    bool IsShared);
