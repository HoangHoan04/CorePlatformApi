using System.Text.Json;
using CorePlatformApi.Application.Common.Models;

namespace CorePlatformApi.Application.Features.Localization;

#region Ngôn ngữ (config.languages)

/// <summary>
/// Bộ lọc danh sách ngôn ngữ.
/// </summary>
public sealed class LanguageQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Ngôn ngữ kèm tiến độ dịch (số khóa đã có bản dịch chuẩn / tổng số khóa).
/// </summary>
public sealed record LanguageDto(
    string Code,
    string Name,
    string NativeName,
    string Direction,
    string DateFormat,
    string? NumberFormat,
    bool IsDefault,
    bool IsActive,
    int SortOrder,
    int TranslatedCount,
    int TotalKeys,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>
/// Ngôn ngữ đang hỗ trợ (cho bộ chọn ngôn ngữ trên giao diện, không cần đăng nhập).
/// </summary>
public sealed record ActiveLanguageDto(string Code, string Name, string NativeName, string Direction, string DateFormat, bool IsDefault);

/// <summary>
/// Tạo / cập nhật ngôn ngữ. Mã (BCP-47) là khóa chính nên không đổi được khi cập nhật.
/// </summary>
public sealed class LanguageSaveRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string Direction { get; set; } = "LTR";
    public string DateFormat { get; set; } = "dd/MM/yyyy";

    /// <summary>
    /// JSON, VD: {"decimal":",","group":"."}
    /// </summary>
    public string? NumberFormat { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public sealed class LanguageCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

#endregion

#region Khóa dịch + bản dịch (config.translation_keys / config.translations)

/// <summary>
/// Bộ lọc màn hình bản dịch.
/// </summary>
public sealed class TranslationQuery : PagedRequest
{
    /// <summary>
    /// Nhóm (đoạn đầu của khóa: common, menu, auth...).
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Ngôn ngữ dùng cho bộ lọc <see cref="Status"/>.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// missing = chưa dịch, translated = đã dịch, unreviewed = đã dịch nhưng chưa duyệt (theo <see cref="LanguageCode"/>).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// WEB, MOBILE, API.
    /// </summary>
    public string? Platform { get; set; }
}

public sealed record TranslationCellDto(Guid Id, string Value, bool IsReviewed, bool IsMachine);

/// <summary>
/// Một dòng của màn hình bản dịch: khóa + giá trị theo từng ngôn ngữ (bản chuẩn, tenant_id NULL).
/// </summary>
public sealed record TranslationRowDto(
    Guid KeyId,
    string FullKey,
    string Namespace,
    string Key,
    string? Description,
    IReadOnlyList<string> Platforms,
    IReadOnlyDictionary<string, TranslationCellDto> Values,
    DateTimeOffset CreatedAt);

/// <summary>
/// Thống kê nhanh cho màn hình bản dịch.
/// </summary>
public sealed record TranslationStatsDto(int TotalKeys, IReadOnlyDictionary<string, int> TranslatedByLanguage);

/// <summary>
/// Đồng bộ khóa dịch từ ứng dụng (danh sách khóa quét được trong mã nguồn + khóa thiếu lúc chạy).
/// Chỉ thêm khóa / bản dịch còn thiếu, không ghi đè bản dịch đã có.
/// </summary>
public sealed class TranslationSyncRequest
{
    /// <summary>
    /// Nền tảng gửi khóa lên (WEB, MOBILE, API) — được thêm vào cột platforms của khóa.
    /// </summary>
    public string? Platform { get; set; }

    public List<TranslationSyncItem> Keys { get; set; } = [];
}

public sealed class TranslationSyncItem
{
    /// <summary>
    /// Khóa đầy đủ, VD: common.actions.save (đoạn đầu là namespace).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Ngữ cảnh cho người dịch (VD: file dùng khóa).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bản dịch mặc định theo mã ngôn ngữ (vi-VN / vi / en-US...). Chỉ ghi khi ngôn ngữ đó chưa có bản dịch.
    /// </summary>
    public Dictionary<string, string>? Values { get; set; }
}

public sealed record TranslationSyncResult(
    int Received,
    int CreatedKeys,
    int ExistingKeys,
    int CreatedTranslations,
    int TotalKeys,
    IReadOnlyList<string> InvalidKeys);

/// <summary>
/// Lưu hàng loạt ô bản dịch. Value rỗng = xóa bản dịch của ngôn ngữ đó.
/// </summary>
public sealed class TranslationSaveRequest
{
    public List<TranslationSaveItem> Items { get; set; } = [];
}

public sealed class TranslationSaveItem
{
    public Guid KeyId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool? IsReviewed { get; set; }
}

public sealed record TranslationSaveResult(int Created, int Updated, int Deleted);

/// <summary>
/// Tạo / sửa khóa dịch (Id rỗng = tạo mới).
/// </summary>
public sealed class TranslationKeySaveRequest
{
    public Guid? Id { get; set; }
    public string FullKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? Platforms { get; set; }

    /// <summary>
    /// Bản dịch kèm theo khi tạo / sửa (mã ngôn ngữ -> nội dung).
    /// </summary>
    public Dictionary<string, string>? Values { get; set; }
}

public sealed class IdsRequest
{
    public List<Guid> Ids { get; set; } = [];
}

/// <summary>
/// Nhập bản dịch của một ngôn ngữ từ JSON (dạng lồng nhau như file ngx-translate hoặc phẳng "a.b.c": "...").
/// </summary>
public sealed class TranslationImportRequest
{
    public string LanguageCode { get; set; } = string.Empty;
    public JsonElement Data { get; set; }

    /// <summary>
    /// Ghi đè bản dịch đã có (mặc định chỉ thêm ô còn trống).
    /// </summary>
    public bool Overwrite { get; set; }

    /// <summary>
    /// Tạo khóa mới nếu khóa trong file chưa có (mặc định bỏ qua khóa lạ).
    /// </summary>
    public bool CreateMissingKeys { get; set; }
}

public sealed record TranslationImportResult(int Total, int CreatedKeys, int Created, int Updated, int Skipped);

#endregion
