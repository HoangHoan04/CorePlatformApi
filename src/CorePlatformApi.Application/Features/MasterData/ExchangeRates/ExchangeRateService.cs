using System.Linq.Expressions;
using CorePlatformApi.Application.Common.Excel;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using K = CorePlatformApi.Application.Common.Localization.MessageKeys;

namespace CorePlatformApi.Application.Features.MasterData.ExchangeRates;

/// <summary>
/// Tỷ giá quy đổi theo thời điểm (bảng exchange_rates) — theo mẫu CountryService.
/// Khác biệt:
///  - Không có trạng thái hoạt động -> "xóa" thay cho kích hoạt / ngừng hoạt động.
///  - Tỷ giá dùng chung (tenant_id NULL, Platform Admin nhập) + tỷ giá riêng của tenant (người dùng tenant nhập).
///    Người dùng tenant xem cả hai, chỉ sửa / xóa tỷ giá của tenant mình; tra cứu ưu tiên tỷ giá riêng.
///  - Có API tra tỷ giá hiệu lực tại thời điểm (dùng cho các module tính tiền).
/// </summary>
public interface IExchangeRateService
{
    Task<PagedResult<ExchangeRateDto>> GetPagedAsync(ExchangeRateQuery query, CancellationToken ct = default);
    Task<ExchangeRateDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<ExchangeRateDto> CreateAsync(ExchangeRateSaveRequest request, CancellationToken ct = default);
    Task<ExchangeRateDto> UpdateAsync(ExchangeRateSaveRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ExchangeRateLookupDto> GetLatestAsync(ExchangeRateLookupQuery query, CancellationToken ct = default);
    Task<byte[]> GetTemplateAsync(CancellationToken ct = default);
    Task<byte[]> ExportAsync(ExchangeRateQuery query, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default);
}

public sealed class ExchangeRateService(
    IApplicationDbContext db,
    IExcelService excel,
    IAppLocalizer localizer,
    ICurrentUserService currentUser) : IExchangeRateService
{
    private const int MaxExportRows = 100_000;

    #region Truy vấn

    public async Task<PagedResult<ExchangeRateDto>> GetPagedAsync(ExchangeRateQuery query, CancellationToken ct = default)
    {
        var q = Sort(Filter(query), query);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(ToDtoExpr).ToListAsync(ct);
        return new PagedResult<ExchangeRateDto>(items, total, pageIndex, pageSize);
    }

    public async Task<ExchangeRateDto> GetAsync(Guid id, CancellationToken ct = default) =>
        await Visible().Where(r => r.Id == id).Select(ToDtoExpr).FirstOrDefaultAsync(ct)
        ?? throw AppException.NotFound(K.ExchangeRate.NotFound);

    /// <summary>
    /// Tỷ giá mới nhất có hiệu lực tại thời điểm <see cref="ExchangeRateLookupQuery.At"/>: ưu tiên tỷ giá riêng của tenant,
    /// rồi tỷ giá dùng chung; không có chiều xuôi thì dùng nghịch đảo chiều ngược.
    /// </summary>
    public async Task<ExchangeRateLookupDto> GetLatestAsync(ExchangeRateLookupQuery query, CancellationToken ct = default)
    {
        var from = (query.From ?? string.Empty).Trim().ToUpperInvariant();
        var to = (query.To ?? string.Empty).Trim().ToUpperInvariant();
        var type = string.IsNullOrWhiteSpace(query.RateType) ? ExchangeRateTypes.Default : query.RateType.Trim().ToUpperInvariant();
        var at = query.At ?? DateTimeOffset.UtcNow;

        if (from == to && from.Length > 0)
        {
            return new ExchangeRateLookupDto(from, to, type, 1m, at, "IDENTITY", false, true);
        }

        var candidates = await Visible().AsNoTracking()
            .Where(r => r.RateType == type && r.EffectiveAt <= at
                        && ((r.FromCurrency == from && r.ToCurrency == to) || (r.FromCurrency == to && r.ToCurrency == from)))
            .OrderByDescending(r => r.TenantId != null)
            .ThenByDescending(r => r.FromCurrency == from)
            .ThenByDescending(r => r.EffectiveAt)
            .FirstOrDefaultAsync(ct)
            ?? throw AppException.NotFound(K.ExchangeRate.NoRate, new { from, to, type });

        var inverse = candidates.FromCurrency != from;
        return new ExchangeRateLookupDto(
            from, to, type,
            inverse ? Math.Round(1m / candidates.Rate, 8) : candidates.Rate,
            candidates.EffectiveAt, candidates.Source, inverse, candidates.TenantId is null);
    }

    /// <summary>
    /// Tỷ giá người dùng được xem: Platform Admin / người dùng không thuộc tenant -> tất cả;
    /// người dùng tenant -> dùng chung + của tenant mình.
    /// </summary>
    private IQueryable<ExchangeRate> Visible()
    {
        var tenantId = currentUser.TenantId;
        var q = db.ExchangeRates.AsQueryable();
        return tenantId is null ? q : q.Where(r => r.TenantId == null || r.TenantId == tenantId);
    }

    private IQueryable<ExchangeRate> Filter(ExchangeRateQuery query)
    {
        var q = Visible().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.FromCurrency))
        {
            var from = query.FromCurrency.Trim().ToUpper();
            q = q.Where(r => r.FromCurrency == from);
        }

        if (!string.IsNullOrWhiteSpace(query.ToCurrency))
        {
            var to = query.ToCurrency.Trim().ToUpper();
            q = q.Where(r => r.ToCurrency == to);
        }

        if (!string.IsNullOrWhiteSpace(query.RateType))
        {
            q = q.Where(r => r.RateType == query.RateType);
        }

        if (!string.IsNullOrWhiteSpace(query.Source))
        {
            var source = query.Source.Trim().ToLower();
            q = q.Where(r => r.Source.ToLower().Contains(source));
        }

        if (query.FromDate.HasValue)
        {
            var start = ToUtc(query.FromDate.Value);
            q = q.Where(r => r.EffectiveAt >= start);
        }

        if (query.ToDate.HasValue)
        {
            var end = ToUtc(query.ToDate.Value.AddDays(1));
            q = q.Where(r => r.EffectiveAt < end);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToUpper();
            q = q.Where(r => r.FromCurrency.Contains(term) || r.ToCurrency.Contains(term));
        }

        return q;
    }

    private static IQueryable<ExchangeRate> Sort(IQueryable<ExchangeRate> q, ExchangeRateQuery query)
    {
        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortField?.ToLowerInvariant()) switch
        {
            "fromcurrency" => desc ? q.OrderByDescending(r => r.FromCurrency) : q.OrderBy(r => r.FromCurrency),
            "tocurrency" => desc ? q.OrderByDescending(r => r.ToCurrency) : q.OrderBy(r => r.ToCurrency),
            "ratetype" => desc ? q.OrderByDescending(r => r.RateType) : q.OrderBy(r => r.RateType),
            "rate" => desc ? q.OrderByDescending(r => r.Rate) : q.OrderBy(r => r.Rate),
            "source" => desc ? q.OrderByDescending(r => r.Source) : q.OrderBy(r => r.Source),
            "effectiveat" => desc ? q.OrderByDescending(r => r.EffectiveAt) : q.OrderBy(r => r.EffectiveAt),
            // Mặc định: mới nhất trước
            _ => q.OrderByDescending(r => r.EffectiveAt).ThenBy(r => r.FromCurrency).ThenBy(r => r.ToCurrency),
        };
    }

    private static readonly Expression<Func<ExchangeRate, ExchangeRateDto>> ToDtoExpr = r => new ExchangeRateDto(
        r.Id, r.TenantId, r.TenantId == null, r.FromCurrency, r.ToCurrency, r.RateType, r.Rate, r.EffectiveAt,
        r.Source, r.Note, r.CreatedAt, r.UpdatedAt);

    private static DateTimeOffset ToUtc(DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local)).ToUniversalTime();

    #endregion

    #region Thêm / sửa / xóa

    public async Task<ExchangeRateDto> CreateAsync(ExchangeRateSaveRequest request, CancellationToken ct = default)
    {
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));
        ThrowIfInvalid(await ValidateDbAsync(data, currentUser.TenantId, null, ct));

        var rate = new ExchangeRate { TenantId = currentUser.TenantId };
        Apply(rate, data);
        db.ExchangeRates.Add(rate);
        await db.SaveChangesAsync(ct);
        return await GetAsync(rate.Id, ct);
    }

    public async Task<ExchangeRateDto> UpdateAsync(ExchangeRateSaveRequest request, CancellationToken ct = default)
    {
        var rate = await FindEditableAsync(request.Id ?? Guid.Empty, ct);
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));
        ThrowIfInvalid(await ValidateDbAsync(data, rate.TenantId, rate.Id, ct));

        Apply(rate, data);
        await db.SaveChangesAsync(ct);
        return await GetAsync(rate.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await FindEditableAsync(id, ct);
        db.ExchangeRates.Remove(rate);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Người dùng tenant không được sửa / xóa tỷ giá dùng chung hoặc của tenant khác.
    /// </summary>
    private async Task<ExchangeRate> FindEditableAsync(Guid id, CancellationToken ct)
    {
        var rate = await Visible().FirstOrDefaultAsync(r => r.Id == id, ct)
                   ?? throw AppException.NotFound(K.ExchangeRate.NotFound);
        if (currentUser.TenantId is { } tenantId && rate.TenantId != tenantId)
        {
            throw AppException.Forbidden(K.ExchangeRate.SharedReadOnly);
        }

        return rate;
    }

    #endregion

    #region Excel

    private static IReadOnlyList<ExcelColumn<ExchangeRateDto>> ExcelColumns() =>
    [
        new(K.ExchangeRate.FromCurrency, x => x.FromCurrency, 12, Required: true, NumberFormat: "@"),
        new(K.ExchangeRate.ToCurrency, x => x.ToCurrency, 12, Required: true, NumberFormat: "@"),
        new(K.ExchangeRate.RateType, x => x.RateType, 16, Required: true, ListValues: ExchangeRateTypes.All),
        new(K.ExchangeRate.Rate, x => x.Rate, 18, Required: true, NumberFormat: "#,##0.########"),
        new(K.ExchangeRate.EffectiveAt, x => x.EffectiveAt, 18, Required: true, NumberFormat: "dd/MM/yyyy HH:mm"),
        new(K.ExchangeRate.Source, x => x.Source, 14),
        new(K.ExchangeRate.Note, x => x.Note, 36),
    ];

    public Task<byte[]> GetTemplateAsync(CancellationToken ct = default) =>
        excel.WriteAsync(K.ExchangeRate.EntityName, ExcelColumns(), [], ct);

    public async Task<byte[]> ExportAsync(ExchangeRateQuery query, CancellationToken ct = default)
    {
        var rows = await Sort(Filter(query), query).Take(MaxExportRows).Select(ToDtoExpr).ToListAsync(ct);
        return await excel.WriteAsync(K.ExchangeRate.EntityName, ExcelColumns(), rows, ct);
    }

    /// <summary>
    /// Nhập vào phạm vi của người nhập (tenant của họ, hoặc dùng chung nếu là Platform Admin).
    /// Trùng (cặp tiền, loại, thời điểm) -> cập nhật tỷ giá; chưa có -> thêm mới.
    /// </summary>
    public async Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var rows = excel.Read(file, ExcelColumns().Count);
        var tenantId = currentUser.TenantId;
        var currencies = (await db.Currencies.AsNoTracking().Select(c => c.Code).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = await db.ExchangeRates.Where(r => r.TenantId == tenantId).ToListAsync(ct);
        var byKey = existing.ToDictionary(r => Key(r.FromCurrency, r.ToCurrency, r.RateType, r.EffectiveAt));
        var seen = new Dictionary<string, int>();

        var messages = new List<string>();
        int created = 0, updated = 0;
        foreach (var row in rows)
        {
            var errors = new List<ValidationError>();
            if (!ExcelValues.TryParseDecimal(row[3], out var rateValue))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.ExchangeRate.Rate }));
            }

            if (!ExcelValues.TryParseDateTime(row[4], out var effectiveAt))
            {
                errors.Add(new(K.Validation.InvalidDate, new { field = K.ExchangeRate.EffectiveAt, value = row[4] }));
            }

            var data = Normalize(new ExchangeRateSaveRequest
            {
                FromCurrency = row[0],
                ToCurrency = row[1],
                RateType = row[2],
                Rate = rateValue ?? 0,
                EffectiveAt = effectiveAt,
                Source = row[5],
                Note = row[6],
            });
            errors.AddRange(Validate(data));

            foreach (var (code, field) in new[] { (data.FromCurrency, K.ExchangeRate.FromCurrency), (data.ToCurrency, K.ExchangeRate.ToCurrency) })
            {
                if (code.Length > 0 && !currencies.Contains(code))
                {
                    errors.Add(new(K.Validation.NotFoundValue, new { field, value = code }));
                }
            }

            if (errors.Count == 0)
            {
                var key = Key(data.FromCurrency, data.ToCurrency, data.RateType, data.EffectiveAt!.Value);
                if (!seen.TryAdd(key, row.RowNumber))
                {
                    errors.Add(new(K.Import.DuplicateInFile, new { field = K.ExchangeRate.EffectiveAt, value = row[4], other = seen[key] }));
                }
            }

            if (errors.Count > 0)
            {
                foreach (var e in errors)
                {
                    messages.Add(await localizer.GetAsync(K.Import.RowError,
                        new { row = row.RowNumber, message = await localizer.GetAsync(e.Key, e.Args, ct) }, ct));
                }

                continue;
            }

            var rowKey = Key(data.FromCurrency, data.ToCurrency, data.RateType, data.EffectiveAt!.Value);
            if (!byKey.TryGetValue(rowKey, out var rate))
            {
                rate = new ExchangeRate { TenantId = tenantId };
                db.ExchangeRates.Add(rate);
                byKey[rowKey] = rate;
                created++;
            }
            else
            {
                updated++;
            }

            Apply(rate, data);
        }

        await db.SaveChangesAsync(ct);
        var success = created + updated;
        return new ImportResult(rows.Count, success, rows.Count - success, messages)
        {
            CreatedCount = created,
            UpdatedCount = updated,
        };
    }

    private static string Key(string from, string to, string type, DateTimeOffset at) =>
        $"{from}|{to}|{type}|{at.UtcTicks}";

    #endregion

    #region Chuẩn hóa + kiểm tra dữ liệu

    private static ExchangeRateSaveRequest Normalize(ExchangeRateSaveRequest r) => new()
    {
        Id = r.Id,
        FromCurrency = (r.FromCurrency ?? string.Empty).Trim().ToUpperInvariant(),
        ToCurrency = (r.ToCurrency ?? string.Empty).Trim().ToUpperInvariant(),
        RateType = string.IsNullOrWhiteSpace(r.RateType) ? ExchangeRateTypes.Default : r.RateType.Trim().ToUpperInvariant(),
        Rate = r.Rate,
        // Bỏ phần giây lẻ để khóa trùng (cặp tiền, loại, thời điểm) ổn định giữa form / Excel
        EffectiveAt = r.EffectiveAt is { } at
            ? new DateTimeOffset(at.UtcDateTime.Year, at.UtcDateTime.Month, at.UtcDateTime.Day,
                at.UtcDateTime.Hour, at.UtcDateTime.Minute, 0, TimeSpan.Zero)
            : null,
        Source = string.IsNullOrWhiteSpace(r.Source) ? "MANUAL" : r.Source.Trim().ToUpperInvariant(),
        Note = ExcelValues.NullIfEmpty(r.Note),
    };

    private static List<ValidationError> Validate(ExchangeRateSaveRequest d)
    {
        var errors = new List<ValidationError>();
        Required(errors, d.FromCurrency, K.ExchangeRate.FromCurrency);
        Required(errors, d.ToCurrency, K.ExchangeRate.ToCurrency);

        if (d.FromCurrency.Length > 0 && d.FromCurrency == d.ToCurrency)
        {
            errors.Add(new(K.ExchangeRate.SameCurrency));
        }

        if (!ExchangeRateTypes.All.Contains(d.RateType))
        {
            errors.Add(new(K.Validation.InvalidValue, new { field = K.ExchangeRate.RateType, value = d.RateType }));
        }

        if (d.Rate <= 0)
        {
            errors.Add(new(K.Validation.MustBePositive, new { field = K.ExchangeRate.Rate }));
        }

        if (!d.EffectiveAt.HasValue)
        {
            errors.Add(new(K.Validation.Required, new { field = K.ExchangeRate.EffectiveAt }));
        }

        if (d.Source is { Length: > 30 })
        {
            errors.Add(new(K.Validation.MaxLength, new { field = K.ExchangeRate.Source, max = 30 }));
        }

        if (d.Note is { Length: > 255 })
        {
            errors.Add(new(K.Validation.MaxLength, new { field = K.ExchangeRate.Note, max = 255 }));
        }

        return errors;
    }

    private async Task<List<ValidationError>> ValidateDbAsync(ExchangeRateSaveRequest d, Guid? tenantId, Guid? selfId, CancellationToken ct)
    {
        var errors = new List<ValidationError>();
        var codes = new[] { d.FromCurrency, d.ToCurrency };
        var found = await db.Currencies.AsNoTracking().Where(c => codes.Contains(c.Code)).Select(c => c.Code).ToListAsync(ct);
        foreach (var (code, field) in new[] { (d.FromCurrency, K.ExchangeRate.FromCurrency), (d.ToCurrency, K.ExchangeRate.ToCurrency) })
        {
            if (!found.Contains(code))
            {
                errors.Add(new(K.Validation.NotFoundValue, new { field, value = code }));
            }
        }

        if (await db.ExchangeRates.AnyAsync(r => r.TenantId == tenantId && r.FromCurrency == d.FromCurrency
                                                 && r.ToCurrency == d.ToCurrency && r.RateType == d.RateType
                                                 && r.EffectiveAt == d.EffectiveAt && r.Id != selfId, ct))
        {
            errors.Add(new(K.ExchangeRate.Exists));
        }

        return errors;
    }

    private static void ThrowIfInvalid(List<ValidationError> errors)
    {
        if (errors.Count > 0)
        {
            var first = errors[0];
            throw first.Key == K.ExchangeRate.Exists
                ? AppException.Conflict("DUPLICATE", first.Key, first.Args)
                : AppException.BadRequest("VALIDATION_FAILED", first.Key, first.Args);
        }
    }

    private static void Required(List<ValidationError> errors, string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new(K.Validation.Required, new { field }));
        }
    }

    private static void Apply(ExchangeRate r, ExchangeRateSaveRequest d)
    {
        r.FromCurrency = d.FromCurrency;
        r.ToCurrency = d.ToCurrency;
        r.RateType = d.RateType;
        r.Rate = d.Rate;
        r.EffectiveAt = d.EffectiveAt!.Value;
        r.Source = d.Source ?? "MANUAL";
        r.Note = d.Note;
    }

    #endregion
}
