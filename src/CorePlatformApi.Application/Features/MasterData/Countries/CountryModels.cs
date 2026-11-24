using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.MasterData.Countries;

/// <summary>
/// Lọc + phân trang danh sách quốc gia (dùng cho cả xuất Excel — xuất đúng những gì đang lọc).
/// </summary>
public sealed class CountryQuery : PagedRequest
{
    /// <summary>Lọc theo mã alpha-2 / alpha-3 (chứa chuỗi).</summary>
    public string? Code { get; set; }

    /// <summary>Lọc theo tên tiếng Việt / tiếng Anh (chứa chuỗi).</summary>
    public string? Name { get; set; }

    public bool? IsActive { get; set; }
}

public sealed record CountryDto(
    Guid Id,
    string Code,
    string Alpha3Code,
    string? NumericCode,
    string Name,
    string NameEn,
    string? PhoneCode,
    string? CurrencyCode,
    string? FlagEmoji,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>
/// Tạo (Id rỗng) / cập nhật (có Id). Mã alpha-2 không đổi được sau khi tạo (được bảng khác tham chiếu).
/// </summary>
public sealed class CountrySaveRequest
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Alpha3Code { get; set; } = string.Empty;
    public string? NumericCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? FlagEmoji { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
