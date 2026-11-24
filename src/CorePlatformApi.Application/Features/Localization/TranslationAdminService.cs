using System.Text.Json;
using System.Text.RegularExpressions;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Domain.Entities.Config;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Features.Localization;

/// <summary>
/// Quản trị khóa dịch + bản dịch chuẩn (tenant_id NULL): đồng bộ khóa từ ứng dụng, nhập bản dịch, nhập/xuất JSON.
/// </summary>
public interface ITranslationAdminService
{
    Task<PagedResult<TranslationRowDto>> GetPagedAsync(TranslationQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<NamespaceCountDto>> GetNamespacesAsync(CancellationToken ct = default);
    Task<TranslationStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<TranslationSyncResult> SyncKeysAsync(TranslationSyncRequest request, CancellationToken ct = default);
    Task<TranslationSaveResult> SaveTranslationsAsync(TranslationSaveRequest request, CancellationToken ct = default);
    Task<TranslationRowDto> SaveKeyAsync(TranslationKeySaveRequest request, CancellationToken ct = default);
    Task<int> DeleteKeysAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<TranslationImportResult> ImportAsync(TranslationImportRequest request, CancellationToken ct = default);
    Task<Dictionary<string, string>> ExportFlatAsync(string language, CancellationToken ct = default);
}

public sealed record NamespaceCountDto(string Namespace, int Count);

public sealed partial class TranslationAdminService(IApplicationDbContext db) : ITranslationAdminService
{
    private static readonly string[] AllPlatforms = ["WEB", "MOBILE", "API"];

    [GeneratedRegex("^[A-Za-z0-9_-]+(\\.[A-Za-z0-9_-]+)+$")]
    private static partial Regex FullKeyRegex();

    #region Danh sách

    public async Task<PagedResult<TranslationRowDto>> GetPagedAsync(TranslationQuery query, CancellationToken ct = default)
    {
        var languages = await LanguageCodesAsync(ct);
        var q = db.TranslationKeys.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Namespace))
        {
            var ns = query.Namespace.Trim();
            q = q.Where(k => k.Namespace == ns);
        }

        if (!string.IsNullOrWhiteSpace(query.Platform))
        {
            var platform = query.Platform.Trim().ToUpperInvariant();
            q = q.Where(k => k.Platforms.Contains(platform));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(k =>
                (k.Namespace + "." + k.Key).ToLower().Contains(term) ||
                (k.Description != null && k.Description.ToLower().Contains(term)) ||
                k.Translations.Any(t => t.TenantId == null && t.Value.ToLower().Contains(term)));
        }

        var language = ResolveLanguage(languages, query.LanguageCode);
        if (language is not null && !string.IsNullOrWhiteSpace(query.Status))
        {
            q = query.Status.Trim().ToLowerInvariant() switch
            {
                "missing" => q.Where(k => !k.Translations.Any(t => t.TenantId == null && t.LanguageCode == language && t.Value != "")),
                "translated" => q.Where(k => k.Translations.Any(t => t.TenantId == null && t.LanguageCode == language && t.Value != "")),
                "unreviewed" => q.Where(k => k.Translations.Any(t => t.TenantId == null && t.LanguageCode == language && t.Value != "" && !t.IsReviewed)),
                _ => q,
            };
        }

        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        q = (query.SortField?.ToLowerInvariant()) switch
        {
            "createdat" => desc ? q.OrderByDescending(k => k.CreatedAt).ThenBy(k => k.Namespace).ThenBy(k => k.Key) : q.OrderBy(k => k.CreatedAt).ThenBy(k => k.Namespace).ThenBy(k => k.Key),
            "description" => desc ? q.OrderByDescending(k => k.Description) : q.OrderBy(k => k.Description),
            _ => desc ? q.OrderByDescending(k => k.Namespace).ThenByDescending(k => k.Key) : q.OrderBy(k => k.Namespace).ThenBy(k => k.Key),
        };

        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var keys = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var rows = await ToRowsAsync(keys, ct);
        return new PagedResult<TranslationRowDto>(rows, total, pageIndex, pageSize);
    }

    public async Task<IReadOnlyList<NamespaceCountDto>> GetNamespacesAsync(CancellationToken ct = default)
    {
        var groups = await db.TranslationKeys.AsNoTracking()
            .GroupBy(k => k.Namespace)
            .Select(g => new { Namespace = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        return groups
            .OrderBy(x => x.Namespace, StringComparer.Ordinal)
            .Select(x => new NamespaceCountDto(x.Namespace, x.Count))
            .ToList();
    }

    public async Task<TranslationStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var total = await db.TranslationKeys.CountAsync(ct);
        var translated = await db.Translations.AsNoTracking()
            .Where(t => t.TenantId == null && t.Value != "")
            .GroupBy(t => t.LanguageCode)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return new TranslationStatsDto(total, translated);
    }

    #endregion

    #region Đồng bộ khóa

    public async Task<TranslationSyncResult> SyncKeysAsync(TranslationSyncRequest request, CancellationToken ct = default)
    {
        var languages = await LanguageCodesAsync(ct);
        var platform = NormalizePlatform(request.Platform);
        var invalid = new List<string>();

        // Gộp các khóa trùng trong request (giữ mô tả / bản dịch đầu tiên có giá trị)
        var incoming = new Dictionary<string, TranslationSyncItem>(StringComparer.Ordinal);
        foreach (var item in request.Keys ?? [])
        {
            var fullKey = item.Key?.Trim() ?? string.Empty;
            if (!TrySplit(fullKey, out _, out _))
            {
                if (!string.IsNullOrEmpty(fullKey))
                {
                    invalid.Add(fullKey);
                }

                continue;
            }

            if (!incoming.TryGetValue(fullKey, out var existing))
            {
                incoming[fullKey] = item;
                continue;
            }

            existing.Description ??= item.Description;
            if (item.Values is { Count: > 0 })
            {
                existing.Values ??= [];
                foreach (var (lang, value) in item.Values)
                {
                    existing.Values.TryAdd(lang, value);
                }
            }
        }

        var allKeys = await db.TranslationKeys.ToListAsync(ct);
        var byFullKey = allKeys.ToDictionary(k => $"{k.Namespace}.{k.Key}", StringComparer.Ordinal);
        var existingCells = (await db.Translations.AsNoTracking()
                .Where(t => t.TenantId == null)
                .Select(t => new { t.KeyId, t.LanguageCode })
                .ToListAsync(ct))
            .Select(x => (x.KeyId, x.LanguageCode))
            .ToHashSet();

        int createdKeys = 0, existingKeys = 0, createdTranslations = 0;
        foreach (var (fullKey, item) in incoming)
        {
            if (byFullKey.TryGetValue(fullKey, out var key))
            {
                existingKeys++;
                if (platform is not null && !key.Platforms.Contains(platform))
                {
                    key.Platforms = [.. key.Platforms, platform];
                }

                if (string.IsNullOrWhiteSpace(key.Description) && !string.IsNullOrWhiteSpace(item.Description))
                {
                    key.Description = item.Description.Trim();
                }
            }
            else
            {
                TrySplit(fullKey, out var ns, out var path);
                key = new TranslationKey
                {
                    Namespace = ns,
                    Key = path,
                    Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                    Platforms = platform is null ? [.. AllPlatforms] : [platform],
                };
                db.TranslationKeys.Add(key);
                byFullKey[fullKey] = key;
                createdKeys++;
            }

            foreach (var (langInput, value) in item.Values ?? [])
            {
                var lang = ResolveLanguage(languages, langInput);
                if (lang is null || string.IsNullOrWhiteSpace(value) || existingCells.Contains((key.Id, lang)))
                {
                    continue;
                }

                db.Translations.Add(new Translation { KeyId = key.Id, LanguageCode = lang, Value = value });
                existingCells.Add((key.Id, lang));
                createdTranslations++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new TranslationSyncResult(
            request.Keys?.Count ?? 0,
            createdKeys,
            existingKeys,
            createdTranslations,
            byFullKey.Count,
            invalid.Distinct().Take(100).ToList());
    }

    #endregion

    #region Nhập bản dịch

    public async Task<TranslationSaveResult> SaveTranslationsAsync(TranslationSaveRequest request, CancellationToken ct = default)
    {
        var items = request.Items ?? [];
        if (items.Count == 0)
        {
            return new TranslationSaveResult(0, 0, 0);
        }

        var languages = await LanguageCodesAsync(ct);
        var keyIds = items.Select(i => i.KeyId).Distinct().ToList();
        var validKeyIds = (await db.TranslationKeys.Where(k => keyIds.Contains(k.Id)).Select(k => k.Id).ToListAsync(ct)).ToHashSet();
        var current = await db.Translations
            .Where(t => t.TenantId == null && keyIds.Contains(t.KeyId))
            .ToListAsync(ct);
        var cells = current.ToDictionary(t => (t.KeyId, t.LanguageCode));

        int created = 0, updated = 0, deleted = 0;
        foreach (var item in items)
        {
            if (!validKeyIds.Contains(item.KeyId))
            {
                throw AppException.NotFound("Khóa dịch không tồn tại (có thể đã bị xóa). Vui lòng tải lại danh sách.");
            }

            var lang = ResolveLanguage(languages, item.LanguageCode)
                       ?? throw AppException.BadRequest("LANGUAGE_NOT_FOUND", $"Ngôn ngữ '{item.LanguageCode}' không tồn tại.");
            cells.TryGetValue((item.KeyId, lang), out var cell);

            if (string.IsNullOrWhiteSpace(item.Value))
            {
                if (cell is not null)
                {
                    db.Translations.Remove(cell);
                    cells.Remove((item.KeyId, lang));
                    deleted++;
                }

                continue;
            }

            if (cell is null)
            {
                cell = new Translation
                {
                    KeyId = item.KeyId,
                    LanguageCode = lang,
                    Value = item.Value,
                    IsReviewed = item.IsReviewed ?? true,
                };
                db.Translations.Add(cell);
                cells[(item.KeyId, lang)] = cell;
                created++;
            }
            else if (cell.Value != item.Value || (item.IsReviewed.HasValue && cell.IsReviewed != item.IsReviewed.Value))
            {
                cell.Value = item.Value;
                cell.IsReviewed = item.IsReviewed ?? true;
                cell.IsMachine = false;
                updated++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new TranslationSaveResult(created, updated, deleted);
    }

    public async Task<TranslationRowDto> SaveKeyAsync(TranslationKeySaveRequest request, CancellationToken ct = default)
    {
        var fullKey = request.FullKey?.Trim() ?? string.Empty;
        if (!TrySplit(fullKey, out var ns, out var path))
        {
            throw AppException.BadRequest(
                "TRANSLATION_KEY_INVALID",
                "Khóa không hợp lệ: dạng namespace.ten_khoa (chữ, số, _, -, ngăn bởi dấu chấm; namespace tối đa 50, phần còn lại tối đa 255 ký tự).");
        }

        TranslationKey key;
        if (request.Id is { } id && id != Guid.Empty)
        {
            key = await db.TranslationKeys.FirstOrDefaultAsync(k => k.Id == id, ct)
                  ?? throw AppException.NotFound("Không tìm thấy khóa dịch.");
        }
        else
        {
            key = new TranslationKey();
            db.TranslationKeys.Add(key);
        }

        if (await db.TranslationKeys.AnyAsync(k => k.Namespace == ns && k.Key == path && k.Id != key.Id, ct))
        {
            throw AppException.Conflict("TRANSLATION_KEY_EXISTS", $"Khóa '{fullKey}' đã tồn tại.");
        }

        key.Namespace = ns;
        key.Key = path;
        key.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var platforms = (request.Platforms ?? [])
            .Select(NormalizePlatform)
            .OfType<string>()
            .Distinct()
            .ToList();
        key.Platforms = platforms.Count > 0 ? platforms : [.. AllPlatforms];
        await db.SaveChangesAsync(ct);

        if (request.Values is { Count: > 0 })
        {
            await SaveTranslationsAsync(new TranslationSaveRequest
            {
                Items = request.Values
                    .Select(v => new TranslationSaveItem { KeyId = key.Id, LanguageCode = v.Key, Value = v.Value })
                    .ToList(),
            }, ct);
        }

        return (await ToRowsAsync([key], ct))[0];
    }

    public async Task<int> DeleteKeysAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
        {
            return 0;
        }

        var list = ids.ToList();
        await db.Translations.Where(t => list.Contains(t.KeyId)).ExecuteDeleteAsync(ct);
        return await db.TranslationKeys.Where(k => list.Contains(k.Id)).ExecuteDeleteAsync(ct);
    }

    #endregion

    #region Nhập / xuất JSON

    public async Task<TranslationImportResult> ImportAsync(TranslationImportRequest request, CancellationToken ct = default)
    {
        var languages = await LanguageCodesAsync(ct);
        var lang = ResolveLanguage(languages, request.LanguageCode)
                   ?? throw AppException.BadRequest("LANGUAGE_NOT_FOUND", $"Ngôn ngữ '{request.LanguageCode}' không tồn tại.");
        if (request.Data.ValueKind != JsonValueKind.Object)
        {
            throw AppException.BadRequest("IMPORT_INVALID", "Dữ liệu nhập phải là một JSON object.");
        }

        var flat = new Dictionary<string, string>(StringComparer.Ordinal);
        Flatten(request.Data, null, flat);

        var keys = await db.TranslationKeys.ToListAsync(ct);
        var byFullKey = keys.ToDictionary(k => $"{k.Namespace}.{k.Key}", StringComparer.Ordinal);
        var cells = await db.Translations.Where(t => t.TenantId == null && t.LanguageCode == lang).ToDictionaryAsync(t => t.KeyId, ct);

        int createdKeys = 0, created = 0, updated = 0, skipped = 0;
        foreach (var (fullKey, value) in flat)
        {
            if (string.IsNullOrWhiteSpace(value) || !TrySplit(fullKey, out var ns, out var path))
            {
                skipped++;
                continue;
            }

            if (!byFullKey.TryGetValue(fullKey, out var key))
            {
                if (!request.CreateMissingKeys)
                {
                    skipped++;
                    continue;
                }

                key = new TranslationKey { Namespace = ns, Key = path, Platforms = [.. AllPlatforms] };
                db.TranslationKeys.Add(key);
                byFullKey[fullKey] = key;
                createdKeys++;
            }

            if (cells.TryGetValue(key.Id, out var cell))
            {
                if (!request.Overwrite || cell.Value == value)
                {
                    skipped++;
                    continue;
                }

                cell.Value = value;
                cell.IsReviewed = false;
                updated++;
            }
            else
            {
                cell = new Translation { KeyId = key.Id, LanguageCode = lang, Value = value };
                db.Translations.Add(cell);
                cells[key.Id] = cell;
                created++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new TranslationImportResult(flat.Count, createdKeys, created, updated, skipped);
    }

    public async Task<Dictionary<string, string>> ExportFlatAsync(string language, CancellationToken ct = default)
    {
        var languages = await LanguageCodesAsync(ct);
        var lang = ResolveLanguage(languages, language)
                   ?? throw AppException.NotFound($"Ngôn ngữ '{language}' không tồn tại.");
        var rows = await db.TranslationKeys.AsNoTracking()
            .OrderBy(k => k.Namespace).ThenBy(k => k.Key)
            .Select(k => new
            {
                FullKey = k.Namespace + "." + k.Key,
                Value = k.Translations.Where(t => t.TenantId == null && t.LanguageCode == lang).Select(t => t.Value).FirstOrDefault(),
            })
            .ToListAsync(ct);

        // Xuất cả khóa chưa dịch (giá trị rỗng) để người dịch điền vào rồi nhập lại
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            result[row.FullKey] = row.Value ?? string.Empty;
        }

        return result;
    }

    private static void Flatten(JsonElement element, string? prefix, Dictionary<string, string> output)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    Flatten(property.Value, prefix is null ? property.Name : $"{prefix}.{property.Name}", output);
                }

                break;
            case JsonValueKind.String:
                if (prefix is not null)
                {
                    output[prefix] = element.GetString() ?? string.Empty;
                }

                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                if (prefix is not null)
                {
                    output[prefix] = element.ToString();
                }

                break;
        }
    }

    #endregion

    #region Tiện ích

    private async Task<List<TranslationRowDto>> ToRowsAsync(List<TranslationKey> keys, CancellationToken ct)
    {
        var ids = keys.Select(k => k.Id).ToList();
        var translations = await db.Translations.AsNoTracking()
            .Where(t => t.TenantId == null && ids.Contains(t.KeyId))
            .ToListAsync(ct);
        var byKey = translations.ToLookup(t => t.KeyId);

        return keys.Select(k => new TranslationRowDto(
                k.Id,
                $"{k.Namespace}.{k.Key}",
                k.Namespace,
                k.Key,
                k.Description,
                k.Platforms,
                byKey[k.Id].ToDictionary(t => t.LanguageCode, t => new TranslationCellDto(t.Id, t.Value, t.IsReviewed, t.IsMachine)),
                k.CreatedAt))
            .ToList();
    }

    private async Task<List<string>> LanguageCodesAsync(CancellationToken ct) =>
        await db.Languages.AsNoTracking().OrderBy(l => l.SortOrder).Select(l => l.Code).ToListAsync(ct);

    /// <summary>
    /// "vi" -> "vi-VN", "en-us" -> "en-US" theo danh sách ngôn ngữ hiện có.
    /// </summary>
    internal static string? ResolveLanguage(IReadOnlyList<string> codes, string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var value = input.Trim().Replace('_', '-');
        return codes.FirstOrDefault(c => string.Equals(c, value, StringComparison.OrdinalIgnoreCase))
               ?? codes.FirstOrDefault(c => c.StartsWith(value + "-", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// "common.actions.save" -> namespace "common", key "actions.save".
    /// </summary>
    internal static bool TrySplit(string fullKey, out string ns, out string key)
    {
        ns = key = string.Empty;
        if (string.IsNullOrWhiteSpace(fullKey) || !FullKeyRegex().IsMatch(fullKey))
        {
            return false;
        }

        var dot = fullKey.IndexOf('.');
        ns = fullKey[..dot];
        key = fullKey[(dot + 1)..];
        return ns.Length <= 50 && key.Length <= 255;
    }

    private static string? NormalizePlatform(string? platform)
    {
        var value = platform?.Trim().ToUpperInvariant();
        return value is not null && AllPlatforms.Contains(value) ? value : null;
    }

    #endregion
}
