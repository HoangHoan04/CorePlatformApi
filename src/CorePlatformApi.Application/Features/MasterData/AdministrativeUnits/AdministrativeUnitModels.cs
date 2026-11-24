using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.MasterData.AdministrativeUnits;

/// <summary>
/// Loại đơn vị hành chính (khớp CHECK unit_type) và cấp tương ứng thường gặp.
/// Mã là dữ liệu — tên hiển thị lấy từ bản dịch <c>enums.adminUnitType.{MÃ}</c>.
/// </summary>
public static class AdminUnitTypes
{
    public static readonly IReadOnlyList<string> All =
    [
        "CENTRAL_CITY", "PROVINCE", "DISTRICT", "URBAN_DISTRICT", "TOWN", "PROVINCIAL_CITY",
        "WARD", "COMMUNE", "TOWNSHIP", "SPECIAL_ZONE",
    ];

    /// <summary>Số cấp tối đa (tỉnh → huyện → xã).</summary>
    public const short MaxLevel = 3;
}

/// <summary>
/// Lọc + phân trang (dùng chung cho xuất Excel).
/// </summary>
public sealed class AdministrativeUnitQuery : PagedRequest
{
    public string? CountryCode { get; set; }

    /// <summary>Chỉ lấy đơn vị con trực tiếp của đơn vị này.</summary>
    public Guid? ParentId { get; set; }

    public short? Level { get; set; }
    public string? UnitType { get; set; }

    /// <summary>Lọc theo mã (chứa chuỗi).</summary>
    public string? Code { get; set; }

    /// <summary>Lọc theo tên / tên đầy đủ / tên tiếng Anh (chứa chuỗi).</summary>
    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>true = chỉ đơn vị còn hiệu lực hôm nay (chưa hết hạn), false = chỉ đơn vị đã hết hiệu lực.</summary>
    public bool? IsEffective { get; set; }
}

/// <summary>
/// Tham số ô chọn đơn vị hành chính (chọn theo tầng: tỉnh → xã).
/// </summary>
public sealed class AdministrativeUnitSelectBoxQuery
{
    public string? CountryCode { get; set; }
    public Guid? ParentId { get; set; }
    public short? Level { get; set; }

    /// <summary>Chỉ lấy đơn vị có cấp ≤ giá trị này (VD chọn đơn vị cấp trên: MaxLevel = 2).</summary>
    public short? MaxLevel { get; set; }

    public string? Search { get; set; }

    /// <summary>Gồm cả đơn vị ngừng dùng / hết hiệu lực (VD chọn đơn vị cũ khi khai báo ánh xạ sáp nhập).</summary>
    public bool IncludeInactive { get; set; }

    public int Take { get; set; } = 200;
}

public sealed record AdministrativeUnitDto(
    Guid Id,
    string CountryCode,
    Guid? ParentId,
    string? ParentCode,
    string? ParentName,
    string Code,
    string Name,
    string FullName,
    string? NameEn,
    string UnitType,
    short Level,
    string? Path,
    string? PhoneAreaCode,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    int SortOrder,
    bool IsActive,
    int ChildCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>
/// Tạo (Id rỗng) / cập nhật (có Id). Quốc gia, đơn vị cấp trên và mã không đổi được sau khi tạo
/// (cấp + đường dẫn cây tính theo đơn vị cấp trên). Sáp nhập / chia tách khai báo ở Ánh xạ sáp nhập.
/// </summary>
public sealed class AdministrativeUnitSaveRequest
{
    public Guid? Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Bỏ trống -> "Tên, Tên đầy đủ của đơn vị cấp trên".</summary>
    public string? FullName { get; set; }

    public string? NameEn { get; set; }
    public string UnitType { get; set; } = string.Empty;
    public string? PhoneAreaCode { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
