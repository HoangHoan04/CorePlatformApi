using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Features.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CorePlatformApi.Application.Common.Localization;

/// <summary>
/// Dịch khóa theo ngôn ngữ của request (header Accept-Language) từ bảng translations — không có văn bản gõ cứng.
/// Chưa có bản dịch ở ngôn ngữ đang dùng -> dùng ngôn ngữ mặc định -> cuối cùng trả nguyên khóa.
/// </summary>
public interface IAppLocalizer
{
    /// <summary>
    /// Văn bản của khóa, thay <c>{{tên}}</c> bằng thuộc tính cùng tên của <paramref name="args"/>.
    /// Chuỗi không có dạng khóa (namespace.key) được trả nguyên văn (vẫn thay tham số).
    /// </summary>
    Task<string> GetAsync(string key, object? args = null, CancellationToken ct = default);
}

public sealed partial class AppLocalizer(IApplicationDbContext db, ICurrentUserService currentUser, IMemoryCache cache) : IAppLocalizer
{
    /// <summary>
    /// Bản dịch được nhớ ngắn hạn: sửa ở màn hình Bản dịch có hiệu lực ở API sau tối đa chừng này thời gian.
    /// </summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    [GeneratedRegex(@"^[a-z][A-Za-z0-9_-]*(\.[A-Za-z0-9_-]+)+$")]
    private static partial Regex KeyRegex();

    [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
    private static partial Regex PlaceholderRegex();

    public static bool IsKey(string? text) => text is not null && KeyRegex().IsMatch(text);

    public async Task<string> GetAsync(string key, object? args = null, CancellationToken ct = default)
    {
        var (current, fallback) = await LoadAsync(ct);
        string Lookup(string k) => current.GetValueOrDefault(k) ?? fallback.GetValueOrDefault(k) ?? k;

        var text = IsKey(key) ? Lookup(key) : key;
        if (text == key && IsKey(key) && args is not null)
        {
            // Khóa chưa có bản dịch: hiện khóa kèm tham số để không mất thông tin (VD số dòng lỗi khi nhập Excel)
            var values = ToDictionary(args);
            return values.Count == 0
                ? key
                : $"{key} [{string.Join("; ", values.Select(v => $"{v.Key}={FormatArg(v.Key, v.Value, Lookup)}"))}]";
        }

        return Format(text, args, Lookup);
    }

    /// <summary>
    /// Tham số mang tên trường / đối tượng (field, entity, from, to) chứa khóa dịch -> dịch luôn.
    /// </summary>
    private static readonly HashSet<string> TranslatableArgs = new(StringComparer.OrdinalIgnoreCase) { "field", "entity", "from", "to" };

    private static string FormatArg(string name, object? value, Func<string, string> lookup)
    {
        var str = Convert.ToString(value) ?? string.Empty;
        return TranslatableArgs.Contains(name) && IsKey(str) ? lookup(str) : str;
    }

    private async Task<(IReadOnlyDictionary<string, string> Current, IReadOnlyDictionary<string, string> Fallback)> LoadAsync(CancellationToken ct)
    {
        var languages = await cache.GetOrCreateAsync("i18n:languages", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await db.Languages.AsNoTracking()
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.IsDefault)
                .Select(l => l.Code)
                .ToListAsync(ct);
        }) ?? [];

        var defaultCode = languages.FirstOrDefault();
        var code = TranslationAdminService.ResolveLanguage(languages, currentUser.Language) ?? defaultCode;
        var current = code is null ? Empty : await LoadLanguageAsync(code, ct);
        var fallback = defaultCode is null || defaultCode == code ? Empty : await LoadLanguageAsync(defaultCode, ct);
        return (current, fallback);
    }

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

    private async Task<IReadOnlyDictionary<string, string>> LoadLanguageAsync(string languageCode, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId;
        return await cache.GetOrCreateAsync($"i18n:{languageCode}:{tenantId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var rows = await db.Translations.AsNoTracking()
                .Where(t => t.LanguageCode == languageCode && t.Value != "" && (t.TenantId == null || t.TenantId == tenantId))
                .Join(db.TranslationKeys, t => t.KeyId, k => k.Id, (t, k) => new { k.Namespace, k.Key, t.Value, t.TenantId })
                .ToListAsync(ct);

            // Bản chung trước, bản riêng của tenant ghi đè
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var row in rows.OrderBy(r => r.TenantId.HasValue))
            {
                map[string.IsNullOrWhiteSpace(row.Namespace) ? row.Key : $"{row.Namespace}.{row.Key}"] = row.Value;
            }

            return (IReadOnlyDictionary<string, string>)map;
        }) ?? Empty;
    }

    /// <summary>
    /// Thay <c>{{tên}}</c>. Tham số <c>field</c> / <c>entity</c> / <c>from</c> / <c>to</c> mang khóa dịch (VD mdm.country.code) được dịch luôn,
    /// để thông báo dạng "{{field}} không được để trống" hiện đúng tên trường theo ngôn ngữ.
    /// </summary>
    private static string Format(string text, object? args, Func<string, string> lookup)
    {
        if (args is null || !text.Contains("{{", StringComparison.Ordinal))
        {
            return text;
        }

        var values = ToDictionary(args);
        return PlaceholderRegex().Replace(text, m =>
        {
            var name = m.Groups[1].Value;
            return values.TryGetValue(name, out var value) ? FormatArg(name, value, lookup) : m.Value;
        });
    }

    private static Dictionary<string, object?> ToDictionary(object args)
    {
        if (args is IDictionary dictionary)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry e in dictionary)
            {
                result[Convert.ToString(e.Key)!] = e.Value;
            }

            return result;
        }

        return args.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p.GetValue(args), StringComparer.OrdinalIgnoreCase);
    }
}
