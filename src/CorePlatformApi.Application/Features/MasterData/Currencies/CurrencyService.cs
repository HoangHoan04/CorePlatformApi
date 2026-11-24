using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CorePlatformApi.Application.Common.Excel;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using K = CorePlatformApi.Application.Common.Localization.MessageKeys;

namespace CorePlatformApi.Application.Features.MasterData.Currencies;

/// <summary>
/// Danh mục tiền tệ ISO 4217 (bảng currencies) — theo mẫu CountryService.
/// </summary>
public interface ICurrencyService
{
    Task<PagedResult<CurrencyDto>> GetPagedAsync(CurrencyQuery query, CancellationToken ct = default);
    Task<CurrencyDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<CurrencyDto> CreateAsync(CurrencySaveRequest request, CancellationToken ct = default);
    Task<CurrencyDto> UpdateAsync(CurrencySaveRequest request, CancellationToken ct = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(string? search, CancellationToken ct = default);
    Task<byte[]> GetTemplateAsync(CancellationToken ct = default);
    Task<byte[]> ExportAsync(CurrencyQuery query, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default);
}

public sealed partial class CurrencyService(
    IApplicationDbContext db,
    IExcelService excel,
    IAppLocalizer localizer) : ICurrencyService
{
    private const int MaxExportRows = 10_000;

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CodeRegex();

    [GeneratedRegex("^[0-9]{3}$")]
    private static partial Regex NumericRegex();

    #region Truy vấn

    public async Task<PagedResult<CurrencyDto>> GetPagedAsync(CurrencyQuery query, CancellationToken ct = default)
    {
        var q = Sort(Filter(query), query);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(ToDtoExpr).ToListAsync(ct);
        return new PagedResult<CurrencyDto>(items, total, pageIndex, pageSize);
    }

    public async Task<CurrencyDto> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.Currencies.AsNoTracking().Where(c => c.Id == id).Select(ToDtoExpr).FirstOrDefaultAsync(ct)
        ?? throw AppException.NotFound(K.Currency.NotFound);

    /// <summary>
    /// Tiền tệ đang hoạt động cho ô chọn (quốc gia, công ty, tỷ giá...).
    /// </summary>
    public async Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(string? search, CancellationToken ct = default)
    {
        var q = db.Currencies.AsNoTracking().Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(term) || c.Name.ToLower().Contains(term));
        }

        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Code)
            .Select(c => new SelectBoxItem(c.Id, c.Code, c.Name))
            .ToListAsync(ct);
    }

    private IQueryable<Currency> Filter(CurrencyQuery query)
    {
        var q = db.Currencies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var code = query.Code.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(code) || (c.NumericCode != null && c.NumericCode.Contains(code)));
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(term) || c.Name.ToLower().Contains(term));
        }

        if (query.IsActive.HasValue)
        {
            q = q.Where(c => c.IsActive == query.IsActive.Value);
        }

        return q;
    }

    private static IQueryable<Currency> Sort(IQueryable<Currency> q, CurrencyQuery query)
    {
        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortField?.ToLowerInvariant()) switch
        {
            "code" => desc ? q.OrderByDescending(c => c.Code) : q.OrderBy(c => c.Code),
            "name" => desc ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
            "decimalplaces" => desc ? q.OrderByDescending(c => c.DecimalPlaces) : q.OrderBy(c => c.DecimalPlaces),
            "isactive" => desc ? q.OrderByDescending(c => c.IsActive) : q.OrderBy(c => c.IsActive),
            "createdat" => desc ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
            _ => desc ? q.OrderByDescending(c => c.SortOrder).ThenBy(c => c.Code) : q.OrderBy(c => c.SortOrder).ThenBy(c => c.Code),
        };
    }

    private static readonly Expression<Func<Currency, CurrencyDto>> ToDtoExpr = c => new CurrencyDto(
        c.Id, c.Code, c.NumericCode, c.Name, c.Symbol, c.DecimalPlaces, c.RoundingUnit, c.SymbolPosition,
        c.SortOrder, c.IsActive, c.CreatedAt, c.UpdatedAt);

    #endregion

    #region Thêm / sửa / trạng thái

    public async Task<CurrencyDto> CreateAsync(CurrencySaveRequest request, CancellationToken ct = default)
    {
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));
        if (await db.Currencies.AnyAsync(c => c.Code == data.Code, ct))
        {
            throw AppException.Conflict("DUPLICATE", K.Currency.CodeExists, new { value = data.Code });
        }

        var currency = new Currency { Code = data.Code };
        Apply(currency, data);
        db.Currencies.Add(currency);
        await db.SaveChangesAsync(ct);
        return await GetAsync(currency.Id, ct);
    }

    public async Task<CurrencyDto> UpdateAsync(CurrencySaveRequest request, CancellationToken ct = default)
    {
        var currency = await FindAsync(request.Id ?? Guid.Empty, ct);
        var data = Normalize(request);
        data.Code = currency.Code; // mã ISO là khóa tham chiếu, không cho sửa
        ThrowIfInvalid(Validate(data));

        Apply(currency, data);
        await db.SaveChangesAsync(ct);
        return await GetAsync(currency.Id, ct);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var currency = await FindAsync(id, ct);
        if (currency.IsActive == isActive)
        {
            return;
        }

        currency.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Currency> FindAsync(Guid id, CancellationToken ct) =>
        await db.Currencies.FirstOrDefaultAsync(c => c.Id == id, ct)
        ?? throw AppException.NotFound(K.Currency.NotFound);

    #endregion

    #region Excel

    private static IReadOnlyList<ExcelColumn<CurrencyDto>> ExcelColumns(string yes, string no) =>
    [
        new(K.Currency.Code, x => x.Code, 10, Required: true, NumberFormat: "@"),
        new(K.Currency.NumericCode, x => x.NumericCode, 12, NumberFormat: "@"),
        new(K.Currency.Name, x => x.Name, 28, Required: true),
        new(K.Currency.Symbol, x => x.Symbol, 10, Required: true),
        new(K.Currency.DecimalPlaces, x => x.DecimalPlaces, 12, NumberFormat: "0"),
        new(K.Currency.RoundingUnit, x => x.RoundingUnit, 14, NumberFormat: "#,##0.######"),
        new(K.Currency.SymbolPosition, x => x.SymbolPosition, 14, ListValues: SymbolPositions.All),
        new(K.Currency.SortOrder, x => x.SortOrder, 10, NumberFormat: "0"),
        new(K.Currency.IsActive, x => x.IsActive ? yes : no, 12, ListValues: [yes, no]),
    ];

    public async Task<byte[]> GetTemplateAsync(CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        return await excel.WriteAsync(K.Currency.EntityName, ExcelColumns(yes, no), [], ct);
    }

    public async Task<byte[]> ExportAsync(CurrencyQuery query, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = await Sort(Filter(query), query).Take(MaxExportRows).Select(ToDtoExpr).ToListAsync(ct);
        return await excel.WriteAsync(K.Currency.EntityName, ExcelColumns(yes, no), rows, ct);
    }

    /// <summary>
    /// Nhập theo mã ISO: chưa có -> thêm mới, đã có -> cập nhật.
    /// </summary>
    public async Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = excel.Read(file, ExcelColumns(yes, no).Count);
        var existing = await db.Currencies.ToDictionaryAsync(c => c.Code, StringComparer.OrdinalIgnoreCase, ct);
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var messages = new List<string>();
        int created = 0, updated = 0;
        foreach (var row in rows)
        {
            var errors = new List<ValidationError>();
            if (!ExcelValues.TryParseInt(row[4], 0, out var decimals))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.Currency.DecimalPlaces }));
            }

            if (!ExcelValues.TryParseDecimal(row[5], out var rounding))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.Currency.RoundingUnit }));
            }

            if (!ExcelValues.TryParseInt(row[7], 0, out var sortOrder))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.Currency.SortOrder }));
            }

            if (!ExcelValues.TryParseBool(row[8], yes, no, true, out var isActive))
            {
                errors.Add(new(K.Validation.InvalidBoolean, new { field = K.Currency.IsActive, yes, no }));
            }

            var data = Normalize(new CurrencySaveRequest
            {
                Code = row[0],
                NumericCode = row[1],
                Name = row[2],
                Symbol = row[3],
                DecimalPlaces = (short)Math.Clamp(decimals, short.MinValue, short.MaxValue),
                RoundingUnit = rounding ?? 1m,
                SymbolPosition = row[6].Length == 0 ? "SUFFIX" : row[6],
                SortOrder = sortOrder,
                IsActive = isActive,
            });
            errors.AddRange(Validate(data));

            if (data.Code.Length > 0 && !seen.TryAdd(data.Code, row.RowNumber))
            {
                errors.Add(new(K.Import.DuplicateInFile, new { field = K.Currency.Code, value = data.Code, other = seen[data.Code] }));
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

            if (!existing.TryGetValue(data.Code, out var currency))
            {
                currency = new Currency { Code = data.Code };
                db.Currencies.Add(currency);
                existing[currency.Code] = currency;
                created++;
            }
            else
            {
                updated++;
            }

            Apply(currency, data);
        }

        await db.SaveChangesAsync(ct);
        var success = created + updated;
        return new ImportResult(rows.Count, success, rows.Count - success, messages)
        {
            CreatedCount = created,
            UpdatedCount = updated,
        };
    }

    private async Task<(string Yes, string No)> YesNoAsync(CancellationToken ct) =>
        (await localizer.GetAsync(K.Common.Yes, ct: ct), await localizer.GetAsync(K.Common.No, ct: ct));

    #endregion

    #region Chuẩn hóa + kiểm tra dữ liệu

    private static CurrencySaveRequest Normalize(CurrencySaveRequest r) => new()
    {
        Id = r.Id,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        NumericCode = ExcelValues.NullIfEmpty(r.NumericCode),
        Name = (r.Name ?? string.Empty).Trim(),
        Symbol = (r.Symbol ?? string.Empty).Trim(),
        DecimalPlaces = r.DecimalPlaces,
        RoundingUnit = r.RoundingUnit,
        SymbolPosition = (r.SymbolPosition ?? string.Empty).Trim().ToUpperInvariant(),
        SortOrder = r.SortOrder,
        IsActive = r.IsActive,
    };

    private static List<ValidationError> Validate(CurrencySaveRequest d)
    {
        var errors = new List<ValidationError>();
        Required(errors, d.Code, K.Currency.Code);
        Required(errors, d.Name, K.Currency.Name);
        Required(errors, d.Symbol, K.Currency.Symbol);

        if (d.Code.Length > 0 && !CodeRegex().IsMatch(d.Code))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.Currency.Code, value = d.Code }));
        }

        if (d.NumericCode is not null && !NumericRegex().IsMatch(d.NumericCode))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.Currency.NumericCode, value = d.NumericCode }));
        }

        if (!SymbolPositions.All.Contains(d.SymbolPosition))
        {
            errors.Add(new(K.Validation.InvalidValue, new { field = K.Currency.SymbolPosition, value = d.SymbolPosition }));
        }

        if (d.DecimalPlaces is < 0 or > 6)
        {
            errors.Add(new(K.Validation.OutOfRange, new { field = K.Currency.DecimalPlaces, min = 0, max = 6 }));
        }

        if (d.RoundingUnit <= 0)
        {
            errors.Add(new(K.Validation.MustBePositive, new { field = K.Currency.RoundingUnit }));
        }

        if (d.Name.Length > 100)
        {
            errors.Add(new(K.Validation.MaxLength, new { field = K.Currency.Name, max = 100 }));
        }

        if (d.Symbol.Length > 10)
        {
            errors.Add(new(K.Validation.MaxLength, new { field = K.Currency.Symbol, max = 10 }));
        }

        return errors;
    }

    private static void ThrowIfInvalid(List<ValidationError> errors)
    {
        if (errors.Count > 0)
        {
            throw AppException.BadRequest("VALIDATION_FAILED", errors[0].Key, errors[0].Args);
        }
    }

    private static void Required(List<ValidationError> errors, string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new(K.Validation.Required, new { field }));
        }
    }

    private static void Apply(Currency c, CurrencySaveRequest d)
    {
        c.NumericCode = d.NumericCode;
        c.Name = d.Name;
        c.Symbol = d.Symbol;
        c.DecimalPlaces = d.DecimalPlaces;
        c.RoundingUnit = d.RoundingUnit;
        c.SymbolPosition = d.SymbolPosition;
        c.SortOrder = d.SortOrder;
        c.IsActive = d.IsActive;
    }

    #endregion
}
