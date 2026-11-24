using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Features.Localization;

public interface ITranslationService
{
    /// <summary>
    /// Bản dịch của một ngôn ngữ dạng object lồng nhau theo namespace + key
    /// (VD: namespace "auth", key "signIn" -> { "auth": { "signIn": "..." } }).
    /// Bản dịch riêng của tenant ghi đè bản dịch chung.
    /// </summary>
    Task<Dictionary<string, object>> GetAsync(string language, Guid? tenantId, CancellationToken ct = default);
}

public sealed class TranslationService(IApplicationDbContext db) : ITranslationService
{
    public async Task<Dictionary<string, object>> GetAsync(string language, Guid? tenantId, CancellationToken ct = default)
    {
        var languageCode = await ResolveLanguageCodeAsync(language, ct);
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        if (languageCode is null)
        {
            return result;
        }

        var rows = await db.Translations.AsNoTracking()
            .Where(t => t.LanguageCode == languageCode && t.Value != "" && (t.TenantId == null || t.TenantId == tenantId))
            .Join(db.TranslationKeys, t => t.KeyId, k => k.Id, (t, k) => new { k.Namespace, k.Key, t.Value, t.TenantId })
            .ToListAsync(ct);

        // Bản chung trước, bản của tenant sau để ghi đè
        var flat = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in rows.OrderBy(r => r.TenantId.HasValue))
        {
            flat[string.IsNullOrWhiteSpace(row.Namespace) ? row.Key : $"{row.Namespace}.{row.Key}"] = row.Value;
        }

        // Sắp theo khóa để khóa cha ("menu.mdm") luôn được đặt trước khóa con ("menu.mdm.tax")
        foreach (var (path, value) in flat.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            SetPath(result, path.Split('.', StringSplitOptions.RemoveEmptyEntries), value);
        }

        return result;
    }

    /// <summary>
    /// "vi" -> "vi-VN", "en" -> "en-US" (theo config.languages); chấp nhận cả mã đầy đủ.
    /// </summary>
    private async Task<string?> ResolveLanguageCodeAsync(string language, CancellationToken ct)
    {
        var languages = await db.Languages.AsNoTracking().Where(l => l.IsActive).Select(l => l.Code).ToListAsync(ct);
        return TranslationAdminService.ResolveLanguage(languages, language);
    }

    /// <summary>
    /// Đặt giá trị theo đường dẫn lồng nhau. Nếu một đoạn giữa đường đã là chuỗi (khóa cha cũng có bản dịch,
    /// VD "menu.mdm" và "menu.mdm.tax"), phần còn lại được ghi thành khóa phẳng "mdm.tax" ở cùng cấp —
    /// ngx-translate vẫn tra được vì nó thử ghép dần các đoạn khi gặp giá trị không phải object.
    /// </summary>
    private static void SetPath(Dictionary<string, object> node, string[] segments, string value)
    {
        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (node.TryGetValue(segments[i], out var child))
            {
                if (child is Dictionary<string, object> childNode)
                {
                    node = childNode;
                    continue;
                }

                node[string.Join('.', segments[i..])] = value;
                return;
            }

            var created = new Dictionary<string, object>(StringComparer.Ordinal);
            node[segments[i]] = created;
            node = created;
        }

        if (segments.Length > 0)
        {
            node[segments[^1]] = value;
        }
    }
}
