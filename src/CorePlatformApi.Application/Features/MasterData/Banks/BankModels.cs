using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.MasterData.Banks;

public sealed class BankQuery : PagedRequest
{
    public string? CountryCode { get; set; }

    /// <summary>Lọc theo mã / BIN / SWIFT (chứa chuỗi).</summary>
    public string? Code { get; set; }

    /// <summary>Lọc theo tên ngắn / tên đầy đủ (chứa chuỗi).</summary>
    public string? Name { get; set; }

    public bool? SupportsVietqr { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class BankSelectBoxQuery
{
    public string? CountryCode { get; set; }
    public string? Search { get; set; }

    /// <summary>Chỉ ngân hàng hỗ trợ VietQR (tạo mã QR chuyển khoản).</summary>
    public bool? SupportsVietqr { get; set; }
}

public sealed record BankDto(
    Guid Id,
    string CountryCode,
    string Code,
    string? Bin,
    string ShortName,
    string Name,
    string? SwiftCode,
    string? LogoUrl,
    bool SupportsVietqr,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed class BankSaveRequest
{
    public Guid? Id { get; set; }
    public string CountryCode { get; set; } = "VN";
    public string Code { get; set; } = string.Empty;
    public string? Bin { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SwiftCode { get; set; }
    public string? LogoUrl { get; set; }
    public bool SupportsVietqr { get; set; }
    public bool IsActive { get; set; } = true;
}
