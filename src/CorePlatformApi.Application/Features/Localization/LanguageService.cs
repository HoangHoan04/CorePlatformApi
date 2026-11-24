using System.Text.Json;
using System.Text.RegularExpressions;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Domain.Entities.Config;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Features.Localization;

/// <summary>
/// Cài đặt ngôn ngữ hệ thống (config.languages).
/// </summary>
public interface ILanguageService
{
    Task<IReadOnlyList<ActiveLanguageDto>> GetActiveAsync(CancellationToken ct = default);
    Task<PagedResult<LanguageDto>> GetPagedAsync(LanguageQuery query, CancellationToken ct = default);
    Task<LanguageDto> GetAsync(string code, CancellationToken ct = default);
    Task<LanguageDto> CreateAsync(LanguageSaveRequest request, CancellationToken ct = default);
    Task<LanguageDto> UpdateAsync(LanguageSaveRequest request, CancellationToken ct = default);
    Task SetActiveAsync(string code, bool isActive, CancellationToken ct = default);
    Task SetDefaultAsync(string code, CancellationToken ct = default);
    Task DeleteAsync(string code, CancellationToken ct = default);
}

public sealed partial class LanguageService(IApplicationDbContext db) : ILanguageService
{
    [GeneratedRegex("^[a-z]{2,3}(-[A-Za-z0-9]{2,8})*$")]
    private static partial Regex CodeRegex();

    public async Task<IReadOnlyList<ActiveLanguageDto>> GetActiveAsync(CancellationToken ct = default) =>
        await db.Languages.AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Code)
            .Select(l => new ActiveLanguageDto(l.Code, l.Name, l.NativeName, l.Direction, l.DateFormat, l.IsDefault))
            .ToListAsync(ct);

    public async Task<PagedResult<LanguageDto>> GetPagedAsync(LanguageQuery query, CancellationToken ct = default)
    {
        var q = db.Languages.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var code = query.Code.Trim().ToLower();
            q = q.Where(l => l.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            q = q.Where(l => l.Name.ToLower().Contains(name) || l.NativeName.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(l => l.Code.ToLower().Contains(term) || l.Name.ToLower().Contains(term) || l.NativeName.ToLower().Contains(term));
        }

        if (query.IsActive.HasValue)
        {
            q = q.Where(l => l.IsActive == query.IsActive.Value);
        }

        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        q = (query.SortField?.ToLowerInvariant()) switch
        {
            "code" => desc ? q.OrderByDescending(l => l.Code) : q.OrderBy(l => l.Code),
            "name" => desc ? q.OrderByDescending(l => l.Name) : q.OrderBy(l => l.Name),
            "nativename" => desc ? q.OrderByDescending(l => l.NativeName) : q.OrderBy(l => l.NativeName),
            "createdat" => desc ? q.OrderByDescending(l => l.CreatedAt) : q.OrderBy(l => l.CreatedAt),
            "isactive" => desc ? q.OrderByDescending(l => l.IsActive) : q.OrderBy(l => l.IsActive),
            "isdefault" => desc ? q.OrderByDescending(l => l.IsDefault) : q.OrderBy(l => l.IsDefault),
            _ => desc ? q.OrderByDescending(l => l.SortOrder).ThenBy(l => l.Code) : q.OrderBy(l => l.SortOrder).ThenBy(l => l.Code),
        };

        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var (totalKeys, translated) = await LoadProgressAsync(items.Select(i => i.Code).ToList(), ct);
        return new PagedResult<LanguageDto>(
            items.Select(l => ToDto(l, translated.GetValueOrDefault(l.Code), totalKeys)).ToList(),
            total,
            pageIndex,
            pageSize);
    }

    public async Task<LanguageDto> GetAsync(string code, CancellationToken ct = default)
    {
        var language = await db.Languages.AsNoTracking().FirstOrDefaultAsync(l => l.Code == code, ct)
                       ?? throw AppException.NotFound($"Không tìm thấy ngôn ngữ '{code}'.");
        var (totalKeys, translated) = await LoadProgressAsync([language.Code], ct);
        return ToDto(language, translated.GetValueOrDefault(language.Code), totalKeys);
    }

    public async Task<LanguageDto> CreateAsync(LanguageSaveRequest request, CancellationToken ct = default)
    {
        var code = NormalizeCode(request.Code);
        if (await db.Languages.AnyAsync(l => l.Code.ToLower() == code.ToLower(), ct))
        {
            throw AppException.Conflict("LANGUAGE_EXISTS", $"Mã ngôn ngữ '{code}' đã tồn tại.");
        }

        var language = new Language { Code = code };
        Apply(language, request);
        // Ngôn ngữ đầu tiên của hệ thống là mặc định
        language.IsDefault = !await db.Languages.AnyAsync(ct);
        if (language.IsDefault)
        {
            language.IsActive = true;
        }

        db.Languages.Add(language);
        await db.SaveChangesAsync(ct);
        return await GetAsync(code, ct);
    }

    public async Task<LanguageDto> UpdateAsync(LanguageSaveRequest request, CancellationToken ct = default)
    {
        var language = await FindAsync(request.Code, ct);
        Apply(language, request);
        if (language.IsDefault && !language.IsActive)
        {
            throw AppException.BadRequest("LANGUAGE_DEFAULT_INACTIVE", "Không thể ngừng hỗ trợ ngôn ngữ mặc định.");
        }

        await db.SaveChangesAsync(ct);
        return await GetAsync(language.Code, ct);
    }

    public async Task SetActiveAsync(string code, bool isActive, CancellationToken ct = default)
    {
        var language = await FindAsync(code, ct);
        if (!isActive && language.IsDefault)
        {
            throw AppException.BadRequest("LANGUAGE_DEFAULT_INACTIVE", "Không thể ngừng hỗ trợ ngôn ngữ mặc định. Hãy chọn ngôn ngữ mặc định khác trước.");
        }

        language.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    public async Task SetDefaultAsync(string code, CancellationToken ct = default)
    {
        var language = await FindAsync(code, ct);
        foreach (var other in await db.Languages.Where(l => l.IsDefault && l.Code != language.Code).ToListAsync(ct))
        {
            other.IsDefault = false;
        }

        language.IsDefault = true;
        language.IsActive = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string code, CancellationToken ct = default)
    {
        var language = await FindAsync(code, ct);
        if (language.IsDefault)
        {
            throw AppException.BadRequest("LANGUAGE_DEFAULT_DELETE", "Không thể xóa ngôn ngữ mặc định.");
        }

        // Bản dịch tham chiếu ngôn ngữ (FK NO ACTION) -> xóa trước
        await db.Translations.Where(t => t.LanguageCode == language.Code).ExecuteDeleteAsync(ct);
        db.Languages.Remove(language);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Language> FindAsync(string code, CancellationToken ct) =>
        await db.Languages.FirstOrDefaultAsync(l => l.Code == code, ct)
        ?? throw AppException.NotFound($"Không tìm thấy ngôn ngữ '{code}'.");

    private async Task<(int TotalKeys, Dictionary<string, int> Translated)> LoadProgressAsync(List<string> codes, CancellationToken ct)
    {
        var totalKeys = await db.TranslationKeys.CountAsync(ct);
        var translated = await db.Translations.AsNoTracking()
            .Where(t => t.TenantId == null && codes.Contains(t.LanguageCode) && t.Value != "")
            .GroupBy(t => t.LanguageCode)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return (totalKeys, translated);
    }

    private static string NormalizeCode(string? code)
    {
        var value = (code ?? string.Empty).Trim().Replace('_', '-');
        var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            throw AppException.BadRequest("LANGUAGE_CODE_REQUIRED", "Vui lòng nhập mã ngôn ngữ.");
        }

        // vi-vn -> vi-VN, zh-hant-tw -> zh-Hant-TW (chuẩn BCP-47)
        parts[0] = parts[0].ToLowerInvariant();
        for (var i = 1; i < parts.Length; i++)
        {
            parts[i] = parts[i].Length switch
            {
                2 => parts[i].ToUpperInvariant(),
                4 => char.ToUpperInvariant(parts[i][0]) + parts[i][1..].ToLowerInvariant(),
                _ => parts[i],
            };
        }

        value = string.Join('-', parts);
        if (value.Length > 10 || !CodeRegex().IsMatch(value))
        {
            throw AppException.BadRequest("LANGUAGE_CODE_INVALID", "Mã ngôn ngữ không hợp lệ (theo BCP-47, tối đa 10 ký tự, VD: vi-VN, en-US, ja-JP).");
        }

        return value;
    }

    private static void Apply(Language language, LanguageSaveRequest request)
    {
        language.Name = Required(request.Name, 100, "tên ngôn ngữ");
        language.NativeName = Required(request.NativeName, 100, "tên bản ngữ");
        language.Direction = string.Equals(request.Direction, "RTL", StringComparison.OrdinalIgnoreCase) ? "RTL" : "LTR";
        language.DateFormat = string.IsNullOrWhiteSpace(request.DateFormat) ? "dd/MM/yyyy" : Required(request.DateFormat, 20, "định dạng ngày");
        language.NumberFormat = NormalizeJson(request.NumberFormat);
        language.IsActive = request.IsActive;
        language.SortOrder = request.SortOrder;
    }

    private static string Required(string? value, int maxLength, string label)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            throw AppException.BadRequest("VALIDATION", $"Vui lòng nhập {label}.");
        }

        if (text.Length > maxLength)
        {
            throw AppException.BadRequest("VALIDATION", $"Trường {label} tối đa {maxLength} ký tự.");
        }

        return text;
    }

    private static string? NormalizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement);
        }
        catch (JsonException)
        {
            throw AppException.BadRequest("VALIDATION", "Định dạng số phải là JSON hợp lệ, VD: {\"decimal\":\",\",\"group\":\".\"}.");
        }
    }

    private static LanguageDto ToDto(Language l, int translated, int totalKeys) => new(
        l.Code, l.Name, l.NativeName, l.Direction, l.DateFormat, l.NumberFormat, l.IsDefault, l.IsActive, l.SortOrder,
        translated, totalKeys, l.CreatedAt, l.UpdatedAt);
}
