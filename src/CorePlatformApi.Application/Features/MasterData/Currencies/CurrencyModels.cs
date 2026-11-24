using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.MasterData.Currencies;

/// <summary>Vị trí ký hiệu tiền tệ (khớp CHECK symbol_position). Tên hiển thị: <c>enums.symbolPosition.{MÃ}</c>.</summary>
public static class SymbolPositions
{
    public static readonly IReadOnlyList<string> All = ["PREFIX", "SUFFIX"];
}

public sealed class CurrencyQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record CurrencyDto(
    Guid Id,
    string Code,
    string? NumericCode,
    string Name,
    string Symbol,
    short DecimalPlaces,
    decimal RoundingUnit,
    string SymbolPosition,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>
/// Tạo (Id rỗng) / cập nhật (có Id). Mã ISO không đổi được sau khi tạo (quốc gia, tỷ giá, công ty tham chiếu theo mã).
/// </summary>
public sealed class CurrencySaveRequest
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? NumericCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public short DecimalPlaces { get; set; }
    public decimal RoundingUnit { get; set; } = 1m;
    public string SymbolPosition { get; set; } = "SUFFIX";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
