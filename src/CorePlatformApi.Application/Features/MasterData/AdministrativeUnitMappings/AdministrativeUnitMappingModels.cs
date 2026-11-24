using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.MasterData.AdministrativeUnitMappings;

/// <summary>
/// Hình thức thay đổi địa giới (khớp CHECK mapping_type). Tên hiển thị: bản dịch <c>enums.adminUnitMappingType.{MÃ}</c>.
/// </summary>
public static class AdminUnitMappingTypes
{
    public static readonly IReadOnlyList<string> All = ["MERGE", "SPLIT", "RENAME", "PARTIAL"];
}

public sealed class AdministrativeUnitMappingQuery : PagedRequest
{
    public string? CountryCode { get; set; }
    public string? MappingType { get; set; }

    /// <summary>Đơn vị cũ hoặc mới là đơn vị này.</summary>
    public Guid? UnitId { get; set; }

    /// <summary>Tìm theo mã / tên đơn vị cũ, mới, số văn bản.</summary>
    public string? Keyword { get; set; }

    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public sealed record AdministrativeUnitMappingDto(
    Guid Id,
    Guid OldUnitId,
    string OldUnitCode,
    string OldUnitName,
    Guid NewUnitId,
    string NewUnitCode,
    string NewUnitName,
    string CountryCode,
    string MappingType,
    DateOnly EffectiveDate,
    string? LegalReference,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed class AdministrativeUnitMappingSaveRequest
{
    public Guid? Id { get; set; }
    public Guid OldUnitId { get; set; }
    public Guid NewUnitId { get; set; }
    public string MappingType { get; set; } = "MERGE";
    public DateOnly? EffectiveDate { get; set; }
    public string? LegalReference { get; set; }
    public string? Note { get; set; }
}
