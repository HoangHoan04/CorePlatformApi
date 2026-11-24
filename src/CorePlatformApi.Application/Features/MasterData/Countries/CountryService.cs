using System.Text.RegularExpressions;
using CorePlatformApi.Application.Common.Excel;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using K = CorePlatformApi.Application.Common.Localization.MessageKeys;

namespace CorePlatformApi.Application.Features.MasterData.Countries;

/// <summary>
/// MẪU CRUD CHUẨN — danh mục quốc gia (bảng countries).
/// Đủ chức năng: danh sách (lọc, sắp xếp, phân trang), chi tiết, tạo mới, chỉnh sửa, kích hoạt / ngừng hoạt động,
/// danh sách cho ô chọn (select-box), file Excel mẫu, xuất Excel, nhập Excel (thêm mới hoặc cập nhật theo mã).
/// Màn hình khác làm theo cùng bố cục: Models + Service (+ cột Excel) + Controller.
/// </summary>
public interface ICountryService
{
    Task<PagedResult<CountryDto>> GetPagedAsync(CountryQuery query, CancellationToken ct = default);
    Task<CountryDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<CountryDto> CreateAsync(CountrySaveRequest request, CancellationToken ct = default);
    Task<CountryDto> UpdateAsync(CountrySaveRequest request, CancellationToken ct = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(string? search, CancellationToken ct = default);
    Task<byte[]> GetTemplateAsync(CancellationToken ct = default);
    Task<byte[]> ExportAsync(CountryQuery query, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default);
}

public sealed partial class CountryService(
    IApplicationDbContext db,
    IExcelService excel,
    IAppLocalizer localizer) : ICountryService
{
    private const int MaxExportRows = 50_000;

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex Alpha2Regex();

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex Alpha3Regex();

    [GeneratedRegex("^[0-9]{3}$")]
    private static partial Regex NumericRegex();

    [GeneratedRegex(@"^\+?[0-9\- ]{1,9}$")]
    private static partial Regex PhoneRegex();

    #region Truy vấn

    public async Task<PagedResult<CountryDto>> GetPagedAsync(CountryQuery query, CancellationToken ct = default)
    {
        var q = Sort(Filter(query), query);

        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(ToDtoExpr).ToListAsync(ct);
        return new PagedResult<CountryDto>(items, total, pageIndex, pageSize);
    }

    public async Task<CountryDto> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.Countries.AsNoTracking().Where(c => c.Id == id).Select(ToDtoExpr).FirstOrDefaultAsync(ct)
        ?? throw AppException.NotFound(K.Country.NotFound);

    /// <summary>
    /// Quốc gia đang hoạt động cho ô chọn ở màn hình khác (công ty, chi nhánh, ngân hàng...).
    /// </summary>
    public async Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(string? search, CancellationToken ct = default)
    {
        var q = db.Countries.AsNoTracking().Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(term) || c.Name.ToLower().Contains(term) || c.NameEn.ToLower().Contains(term));
        }

        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new SelectBoxItem(c.Id, c.Code, c.Name))
            .ToListAsync(ct);
    }

    private IQueryable<Country> Filter(CountryQuery query)
    {
        var q = db.Countries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var code = query.Code.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(code) || c.Alpha3Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(name) || c.NameEn.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(term) || c.Alpha3Code.ToLower().Contains(term)
                             || c.Name.ToLower().Contains(term) || c.NameEn.ToLower().Contains(term));
        }

        if (query.IsActive.HasValue)
        {
            q = q.Where(c => c.IsActive == query.IsActive.Value);
        }

        return q;
    }

    private static IQueryable<Country> Sort(IQueryable<Country> q, CountryQuery query)
    {
        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortField?.ToLowerInvariant()) switch
        {
            "code" => desc ? q.OrderByDescending(c => c.Code) : q.OrderBy(c => c.Code),
            "alpha3code" => desc ? q.OrderByDescending(c => c.Alpha3Code) : q.OrderBy(c => c.Alpha3Code),
            "name" => desc ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
            "nameen" => desc ? q.OrderByDescending(c => c.NameEn) : q.OrderBy(c => c.NameEn),
            "isactive" => desc ? q.OrderByDescending(c => c.IsActive) : q.OrderBy(c => c.IsActive),
            "createdat" => desc ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
            _ => desc ? q.OrderByDescending(c => c.SortOrder).ThenBy(c => c.Name) : q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name),
        };
    }

    private static readonly System.Linq.Expressions.Expression<Func<Country, CountryDto>> ToDtoExpr = c => new CountryDto(
        c.Id, c.Code, c.Alpha3Code, c.NumericCode, c.Name, c.NameEn, c.PhoneCode, c.CurrencyCode, c.FlagEmoji,
        c.SortOrder, c.IsActive, c.CreatedAt, c.UpdatedAt);

    #endregion

    #region Thêm / sửa / trạng thái

    public async Task<CountryDto> CreateAsync(CountrySaveRequest request, CancellationToken ct = default)
    {
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));
        ThrowIfInvalid(await ValidateUniqueAsync(data, null, ct));

        var country = new Country { Code = data.Code };
        Apply(country, data);
        db.Countries.Add(country);
        await db.SaveChangesAsync(ct);
        return await GetAsync(country.Id, ct);
    }

    public async Task<CountryDto> UpdateAsync(CountrySaveRequest request, CancellationToken ct = default)
    {
        var country = await FindAsync(request.Id ?? Guid.Empty, ct);
        var data = Normalize(request);
        data.Code = country.Code; // mã alpha-2 là khóa tham chiếu, không cho sửa
        ThrowIfInvalid(Validate(data));
        ThrowIfInvalid(await ValidateUniqueAsync(data, country.Id, ct));

        Apply(country, data);
        await db.SaveChangesAsync(ct);
        return await GetAsync(country.Id, ct);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var country = await FindAsync(id, ct);
        if (country.IsActive == isActive)
        {
            return;
        }

        country.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Country> FindAsync(Guid id, CancellationToken ct) =>
        await db.Countries.FirstOrDefaultAsync(c => c.Id == id, ct)
        ?? throw AppException.NotFound(K.Country.NotFound);

    #endregion

    #region Excel

    /// <summary>
    /// Cột Excel — dùng chung cho file mẫu, xuất và nhập (nhập đọc theo đúng thứ tự cột này).
    /// </summary>
    private IReadOnlyList<ExcelColumn<CountryDto>> ExcelColumns(string yes, string no) =>
    [
        new(K.Country.Code, x => x.Code, 12, Required: true, NumberFormat: "@"),
        new(K.Country.Alpha3Code, x => x.Alpha3Code, 12, Required: true, NumberFormat: "@"),
        new(K.Country.NumericCode, x => x.NumericCode, 12, NumberFormat: "@"),
        new(K.Country.Name, x => x.Name, 30, Required: true),
        new(K.Country.NameEn, x => x.NameEn, 30, Required: true),
        new(K.Country.PhoneCode, x => x.PhoneCode, 14, NumberFormat: "@"),
        new(K.Country.CurrencyCode, x => x.CurrencyCode, 14, NumberFormat: "@"),
        new(K.Country.FlagEmoji, x => x.FlagEmoji, 10),
        new(K.Country.SortOrder, x => x.SortOrder, 12, NumberFormat: "0"),
        new(K.Country.IsActive, x => x.IsActive ? yes : no, 14, ListValues: [yes, no]),
    ];

    public async Task<byte[]> GetTemplateAsync(CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        return await excel.WriteAsync(K.Country.EntityName, ExcelColumns(yes, no), [], ct);
    }

    public async Task<byte[]> ExportAsync(CountryQuery query, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = await Sort(Filter(query), query).Take(MaxExportRows).Select(ToDtoExpr).ToListAsync(ct);
        return await excel.WriteAsync(K.Country.EntityName, ExcelColumns(yes, no), rows, ct);
    }

    /// <summary>
    /// Nhập theo mã alpha-2: chưa có -> thêm mới, đã có -> cập nhật. Dòng lỗi được bỏ qua và báo lại,
    /// các dòng hợp lệ vẫn được lưu (một lần SaveChanges).
    /// </summary>
    public async Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var columns = ExcelColumns(yes, no);
        var rows = excel.Read(file, columns.Count);

        var existing = await db.Countries.ToDictionaryAsync(c => c.Code, StringComparer.OrdinalIgnoreCase, ct);
        var currencies = (await db.Currencies.AsNoTracking().Select(c => c.Code).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seenCodes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var seenAlpha3 = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var errors = new List<string>();
        int created = 0, updated = 0;
        foreach (var row in rows)
        {
            var rowErrors = new List<ValidationError>();

            if (!ExcelValues.TryParseInt(row[8], 0, out var sortOrder))
            {
                rowErrors.Add(new(K.Validation.InvalidNumber, new { field = K.Country.SortOrder }));
            }

            if (!ExcelValues.TryParseBool(row[9], yes, no, true, out var isActive))
            {
                rowErrors.Add(new(K.Validation.InvalidBoolean, new { field = K.Country.IsActive, yes, no }));
            }

            var data = Normalize(new CountrySaveRequest
            {
                Code = row[0],
                Alpha3Code = row[1],
                NumericCode = ExcelValues.NullIfEmpty(row[2]),
                Name = row[3],
                NameEn = row[4],
                PhoneCode = ExcelValues.NullIfEmpty(row[5]),
                CurrencyCode = ExcelValues.NullIfEmpty(row[6]),
                FlagEmoji = ExcelValues.NullIfEmpty(row[7]),
                SortOrder = sortOrder,
                IsActive = isActive,
            });
            rowErrors.AddRange(Validate(data));

            // Trùng trong chính file
            if (data.Code.Length > 0 && !seenCodes.TryAdd(data.Code, row.RowNumber))
            {
                rowErrors.Add(new(K.Import.DuplicateInFile, new { field = K.Country.Code, value = data.Code, other = seenCodes[data.Code] }));
            }

            if (data.Alpha3Code.Length > 0 && !seenAlpha3.TryAdd(data.Alpha3Code, row.RowNumber))
            {
                rowErrors.Add(new(K.Import.DuplicateInFile, new { field = K.Country.Alpha3Code, value = data.Alpha3Code, other = seenAlpha3[data.Alpha3Code] }));
            }

            existing.TryGetValue(data.Code, out var country);

            // Trùng với dữ liệu đã có (alpha-3 thuộc quốc gia khác)
            if (data.Alpha3Code.Length > 0 && existing.Values.Any(c =>
                    c.Alpha3Code.Equals(data.Alpha3Code, StringComparison.OrdinalIgnoreCase) && c != country))
            {
                rowErrors.Add(new(K.Country.Alpha3Exists, new { value = data.Alpha3Code }));
            }

            if (data.CurrencyCode is not null && !currencies.Contains(data.CurrencyCode))
            {
                rowErrors.Add(new(K.Validation.NotFoundValue, new { field = K.Country.CurrencyCode, value = data.CurrencyCode }));
            }

            if (rowErrors.Count > 0)
            {
                foreach (var e in rowErrors)
                {
                    errors.Add(await localizer.GetAsync(K.Import.RowError,
                        new { row = row.RowNumber, message = await localizer.GetAsync(e.Key, e.Args, ct) }, ct));
                }

                continue;
            }

            if (country is null)
            {
                country = new Country { Code = data.Code };
                db.Countries.Add(country);
                existing[country.Code] = country;
                created++;
            }
            else
            {
                updated++;
            }

            Apply(country, data);
        }

        await db.SaveChangesAsync(ct);
        var success = created + updated;
        return new ImportResult(rows.Count, success, rows.Count - success, errors)
        {
            CreatedCount = created,
            UpdatedCount = updated,
        };
    }

    private async Task<(string Yes, string No)> YesNoAsync(CancellationToken ct) =>
        (await localizer.GetAsync(K.Common.Yes, ct: ct), await localizer.GetAsync(K.Common.No, ct: ct));

    #endregion

    #region Chuẩn hóa + kiểm tra dữ liệu (dùng chung cho form và nhập Excel)

    private static CountrySaveRequest Normalize(CountrySaveRequest r) => new()
    {
        Id = r.Id,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Alpha3Code = (r.Alpha3Code ?? string.Empty).Trim().ToUpperInvariant(),
        NumericCode = ExcelValues.NullIfEmpty(r.NumericCode),
        Name = (r.Name ?? string.Empty).Trim(),
        NameEn = (r.NameEn ?? string.Empty).Trim(),
        PhoneCode = ExcelValues.NullIfEmpty(r.PhoneCode) is { } phone && !phone.StartsWith('+') ? "+" + phone : ExcelValues.NullIfEmpty(r.PhoneCode),
        CurrencyCode = ExcelValues.NullIfEmpty(r.CurrencyCode)?.ToUpperInvariant(),
        FlagEmoji = ExcelValues.NullIfEmpty(r.FlagEmoji),
        SortOrder = r.SortOrder,
        IsActive = r.IsActive,
    };

    /// <summary>
    /// Kiểm tra định dạng / bắt buộc / độ dài — không truy vấn database.
    /// </summary>
    private static List<ValidationError> Validate(CountrySaveRequest d)
    {
        var errors = new List<ValidationError>();
        Required(errors, d.Code, K.Country.Code);
        Required(errors, d.Alpha3Code, K.Country.Alpha3Code);
        Required(errors, d.Name, K.Country.Name);
        Required(errors, d.NameEn, K.Country.NameEn);

        Format(errors, d.Code, Alpha2Regex(), K.Country.Code);
        Format(errors, d.Alpha3Code, Alpha3Regex(), K.Country.Alpha3Code);
        Format(errors, d.NumericCode, NumericRegex(), K.Country.NumericCode);
        Format(errors, d.PhoneCode, PhoneRegex(), K.Country.PhoneCode);
        Format(errors, d.CurrencyCode, Alpha3Regex(), K.Country.CurrencyCode);

        MaxLength(errors, d.Name, 150, K.Country.Name);
        MaxLength(errors, d.NameEn, 150, K.Country.NameEn);
        MaxLength(errors, d.FlagEmoji, 10, K.Country.FlagEmoji);
        return errors;
    }

    /// <summary>
    /// Kiểm tra trùng mã + tiền tệ tồn tại (form). Nhập Excel kiểm tra hàng loạt trong bộ nhớ.
    /// </summary>
    private async Task<List<ValidationError>> ValidateUniqueAsync(CountrySaveRequest d, Guid? selfId, CancellationToken ct)
    {
        var errors = new List<ValidationError>();
        if (selfId is null && await db.Countries.AnyAsync(c => c.Code == d.Code, ct))
        {
            errors.Add(new(K.Country.CodeExists, new { value = d.Code }));
        }

        if (await db.Countries.AnyAsync(c => c.Alpha3Code == d.Alpha3Code && c.Id != selfId, ct))
        {
            errors.Add(new(K.Country.Alpha3Exists, new { value = d.Alpha3Code }));
        }

        if (d.CurrencyCode is not null && !await db.Currencies.AnyAsync(c => c.Code == d.CurrencyCode, ct))
        {
            errors.Add(new(K.Validation.NotFoundValue, new { field = K.Country.CurrencyCode, value = d.CurrencyCode }));
        }

        return errors;
    }

    private static void ThrowIfInvalid(List<ValidationError> errors)
    {
        if (errors.Count > 0)
        {
            var first = errors[0];
            throw first.Key is K.Country.CodeExists or K.Country.Alpha3Exists
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

    private static void Format(List<ValidationError> errors, string? value, Regex regex, string field)
    {
        if (!string.IsNullOrEmpty(value) && !regex.IsMatch(value))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field, value }));
        }
    }

    private static void MaxLength(List<ValidationError> errors, string? value, int max, string field)
    {
        if (value is not null && value.Length > max)
        {
            errors.Add(new(K.Validation.MaxLength, new { field, max }));
        }
    }

    private static void Apply(Country c, CountrySaveRequest d)
    {
        c.Alpha3Code = d.Alpha3Code;
        c.NumericCode = d.NumericCode;
        c.Name = d.Name;
        c.NameEn = d.NameEn;
        c.PhoneCode = d.PhoneCode;
        c.CurrencyCode = d.CurrencyCode;
        c.FlagEmoji = d.FlagEmoji;
        c.SortOrder = d.SortOrder;
        c.IsActive = d.IsActive;
    }

    #endregion
}
