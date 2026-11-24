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

namespace CorePlatformApi.Application.Features.MasterData.Banks;

/// <summary>
/// Danh mục ngân hàng (bảng banks — VietQR, chi lương, thanh toán NCC) — theo mẫu CountryService.
/// Nhập Excel theo mã ngân hàng (thêm mới / cập nhật).
/// </summary>
public interface IBankService
{
    Task<PagedResult<BankDto>> GetPagedAsync(BankQuery query, CancellationToken ct = default);
    Task<BankDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<BankDto> CreateAsync(BankSaveRequest request, CancellationToken ct = default);
    Task<BankDto> UpdateAsync(BankSaveRequest request, CancellationToken ct = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(BankSelectBoxQuery query, CancellationToken ct = default);
    Task<byte[]> GetTemplateAsync(CancellationToken ct = default);
    Task<byte[]> ExportAsync(BankQuery query, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default);
}

public sealed partial class BankService(
    IApplicationDbContext db,
    IExcelService excel,
    IAppLocalizer localizer) : IBankService
{
    private const int MaxExportRows = 10_000;

    [GeneratedRegex("^[A-Z0-9_-]{1,20}$")]
    private static partial Regex CodeRegex();

    [GeneratedRegex("^[0-9]{6}$")]
    private static partial Regex BinRegex();

    /// <summary>SWIFT/BIC: 4 chữ ngân hàng + 2 chữ quốc gia + 2 ký tự địa điểm [+ 3 ký tự chi nhánh].</summary>
    [GeneratedRegex("^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$")]
    private static partial Regex SwiftRegex();

    #region Truy vấn

    public async Task<PagedResult<BankDto>> GetPagedAsync(BankQuery query, CancellationToken ct = default)
    {
        var q = Sort(Filter(query), query);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(ToDtoExpr).ToListAsync(ct);
        return new PagedResult<BankDto>(items, total, pageIndex, pageSize);
    }

    public async Task<BankDto> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.Banks.AsNoTracking().Where(b => b.Id == id).Select(ToDtoExpr).FirstOrDefaultAsync(ct)
        ?? throw AppException.NotFound(K.Bank.NotFound);

    /// <summary>
    /// Ngân hàng đang hoạt động cho ô chọn. Name = "Tên ngắn - Tên đầy đủ" để dễ tìm.
    /// </summary>
    public async Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(BankSelectBoxQuery query, CancellationToken ct = default)
    {
        var q = db.Banks.AsNoTracking().Where(b => b.IsActive);
        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var country = query.CountryCode.Trim().ToUpper();
            q = q.Where(b => b.CountryCode == country);
        }

        if (query.SupportsVietqr.HasValue)
        {
            q = q.Where(b => b.SupportsVietqr == query.SupportsVietqr.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(b => b.Code.ToLower().Contains(term) || b.ShortName.ToLower().Contains(term)
                             || b.Name.ToLower().Contains(term) || (b.Bin != null && b.Bin.Contains(term)));
        }

        return await q.OrderBy(b => b.ShortName)
            .Select(b => new SelectBoxItem(b.Id, b.Code, b.ShortName + " - " + b.Name))
            .ToListAsync(ct);
    }

    private IQueryable<Bank> Filter(BankQuery query)
    {
        var q = db.Banks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var country = query.CountryCode.Trim().ToUpper();
            q = q.Where(b => b.CountryCode == country);
        }

        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var code = query.Code.Trim().ToLower();
            q = q.Where(b => b.Code.ToLower().Contains(code) || (b.Bin != null && b.Bin.Contains(code))
                             || (b.SwiftCode != null && b.SwiftCode.ToLower().Contains(code)));
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            q = q.Where(b => b.ShortName.ToLower().Contains(name) || b.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(b => b.Code.ToLower().Contains(term) || b.ShortName.ToLower().Contains(term) || b.Name.ToLower().Contains(term));
        }

        if (query.SupportsVietqr.HasValue)
        {
            q = q.Where(b => b.SupportsVietqr == query.SupportsVietqr.Value);
        }

        if (query.IsActive.HasValue)
        {
            q = q.Where(b => b.IsActive == query.IsActive.Value);
        }

        return q;
    }

    private static IQueryable<Bank> Sort(IQueryable<Bank> q, BankQuery query)
    {
        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortField?.ToLowerInvariant()) switch
        {
            "code" => desc ? q.OrderByDescending(b => b.Code) : q.OrderBy(b => b.Code),
            "bin" => desc ? q.OrderByDescending(b => b.Bin) : q.OrderBy(b => b.Bin),
            "name" => desc ? q.OrderByDescending(b => b.Name) : q.OrderBy(b => b.Name),
            "supportsvietqr" => desc ? q.OrderByDescending(b => b.SupportsVietqr) : q.OrderBy(b => b.SupportsVietqr),
            "isactive" => desc ? q.OrderByDescending(b => b.IsActive) : q.OrderBy(b => b.IsActive),
            "createdat" => desc ? q.OrderByDescending(b => b.CreatedAt) : q.OrderBy(b => b.CreatedAt),
            _ => desc ? q.OrderByDescending(b => b.ShortName) : q.OrderBy(b => b.ShortName),
        };
    }

    private static readonly Expression<Func<Bank, BankDto>> ToDtoExpr = b => new BankDto(
        b.Id, b.CountryCode, b.Code, b.Bin, b.ShortName, b.Name, b.SwiftCode, b.LogoUrl, b.SupportsVietqr,
        b.IsActive, b.CreatedAt, b.UpdatedAt);

    #endregion

    #region Thêm / sửa / trạng thái

    public async Task<BankDto> CreateAsync(BankSaveRequest request, CancellationToken ct = default)
    {
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));
        ThrowIfInvalid(await ValidateDbAsync(data, null, ct));

        var bank = new Bank();
        Apply(bank, data);
        db.Banks.Add(bank);
        await db.SaveChangesAsync(ct);
        return await GetAsync(bank.Id, ct);
    }

    public async Task<BankDto> UpdateAsync(BankSaveRequest request, CancellationToken ct = default)
    {
        var bank = await FindAsync(request.Id ?? Guid.Empty, ct);
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));
        ThrowIfInvalid(await ValidateDbAsync(data, bank.Id, ct));

        Apply(bank, data);
        await db.SaveChangesAsync(ct);
        return await GetAsync(bank.Id, ct);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var bank = await FindAsync(id, ct);
        if (bank.IsActive == isActive)
        {
            return;
        }

        bank.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Bank> FindAsync(Guid id, CancellationToken ct) =>
        await db.Banks.FirstOrDefaultAsync(b => b.Id == id, ct)
        ?? throw AppException.NotFound(K.Bank.NotFound);

    #endregion

    #region Excel

    private static IReadOnlyList<ExcelColumn<BankDto>> ExcelColumns(string yes, string no) =>
    [
        new(K.Bank.CountryCode, x => x.CountryCode, 10, Required: true, NumberFormat: "@"),
        new(K.Bank.Code, x => x.Code, 12, Required: true, NumberFormat: "@"),
        new(K.Bank.Bin, x => x.Bin, 10, NumberFormat: "@"),
        new(K.Bank.ShortName, x => x.ShortName, 20, Required: true),
        new(K.Bank.Name, x => x.Name, 50, Required: true),
        new(K.Bank.SwiftCode, x => x.SwiftCode, 14, NumberFormat: "@"),
        new(K.Bank.LogoUrl, x => x.LogoUrl, 40),
        new(K.Bank.SupportsVietqr, x => x.SupportsVietqr ? yes : no, 12, ListValues: [yes, no]),
        new(K.Bank.IsActive, x => x.IsActive ? yes : no, 12, ListValues: [yes, no]),
    ];

    public async Task<byte[]> GetTemplateAsync(CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        return await excel.WriteAsync(K.Bank.EntityName, ExcelColumns(yes, no), [], ct);
    }

    public async Task<byte[]> ExportAsync(BankQuery query, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = await Sort(Filter(query), query).Take(MaxExportRows).Select(ToDtoExpr).ToListAsync(ct);
        return await excel.WriteAsync(K.Bank.EntityName, ExcelColumns(yes, no), rows, ct);
    }

    /// <summary>
    /// Nhập theo mã ngân hàng: chưa có -> thêm mới, đã có -> cập nhật.
    /// </summary>
    public async Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = excel.Read(file, ExcelColumns(yes, no).Count);
        var countries = (await db.Countries.AsNoTracking().Select(c => c.Code).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = await db.Banks.ToDictionaryAsync(b => b.Code, StringComparer.OrdinalIgnoreCase, ct);
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var messages = new List<string>();
        int created = 0, updated = 0;
        foreach (var row in rows)
        {
            var errors = new List<ValidationError>();
            if (!ExcelValues.TryParseBool(row[7], yes, no, false, out var vietqr))
            {
                errors.Add(new(K.Validation.InvalidBoolean, new { field = K.Bank.SupportsVietqr, yes, no }));
            }

            if (!ExcelValues.TryParseBool(row[8], yes, no, true, out var isActive))
            {
                errors.Add(new(K.Validation.InvalidBoolean, new { field = K.Bank.IsActive, yes, no }));
            }

            var data = Normalize(new BankSaveRequest
            {
                CountryCode = row[0],
                Code = row[1],
                Bin = row[2],
                ShortName = row[3],
                Name = row[4],
                SwiftCode = row[5],
                LogoUrl = row[6],
                SupportsVietqr = vietqr,
                IsActive = isActive,
            });
            errors.AddRange(Validate(data));

            if (data.CountryCode.Length > 0 && !countries.Contains(data.CountryCode))
            {
                errors.Add(new(K.Validation.NotFoundValue, new { field = K.Bank.CountryCode, value = data.CountryCode }));
            }

            if (data.Code.Length > 0 && !seen.TryAdd(data.Code, row.RowNumber))
            {
                errors.Add(new(K.Import.DuplicateInFile, new { field = K.Bank.Code, value = data.Code, other = seen[data.Code] }));
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

            if (!existing.TryGetValue(data.Code, out var bank))
            {
                bank = new Bank();
                db.Banks.Add(bank);
                existing[data.Code] = bank;
                created++;
            }
            else
            {
                updated++;
            }

            Apply(bank, data);
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

    private static BankSaveRequest Normalize(BankSaveRequest r) => new()
    {
        Id = r.Id,
        CountryCode = (string.IsNullOrWhiteSpace(r.CountryCode) ? "VN" : r.CountryCode).Trim().ToUpperInvariant(),
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Bin = ExcelValues.NullIfEmpty(r.Bin),
        ShortName = (r.ShortName ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        SwiftCode = ExcelValues.NullIfEmpty(r.SwiftCode)?.ToUpperInvariant(),
        LogoUrl = ExcelValues.NullIfEmpty(r.LogoUrl),
        SupportsVietqr = r.SupportsVietqr,
        IsActive = r.IsActive,
    };

    private static List<ValidationError> Validate(BankSaveRequest d)
    {
        var errors = new List<ValidationError>();
        Required(errors, d.Code, K.Bank.Code);
        Required(errors, d.ShortName, K.Bank.ShortName);
        Required(errors, d.Name, K.Bank.Name);

        if (d.Code.Length > 0 && !CodeRegex().IsMatch(d.Code))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.Bank.Code, value = d.Code }));
        }

        if (d.Bin is not null && !BinRegex().IsMatch(d.Bin))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.Bank.Bin, value = d.Bin }));
        }

        if (d.SwiftCode is not null && !SwiftRegex().IsMatch(d.SwiftCode))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.Bank.SwiftCode, value = d.SwiftCode }));
        }

        if (d.LogoUrl is not null
            && !(Uri.TryCreate(d.LogoUrl, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.Bank.LogoUrl, value = d.LogoUrl }));
        }

        // VietQR cần mã BIN NAPAS
        if (d.SupportsVietqr && d.Bin is null)
        {
            errors.Add(new(K.Validation.Required, new { field = K.Bank.Bin }));
        }

        MaxLength(errors, d.ShortName, 100, K.Bank.ShortName);
        MaxLength(errors, d.Name, 255, K.Bank.Name);
        MaxLength(errors, d.LogoUrl, 500, K.Bank.LogoUrl);
        return errors;
    }

    private async Task<List<ValidationError>> ValidateDbAsync(BankSaveRequest d, Guid? selfId, CancellationToken ct)
    {
        var errors = new List<ValidationError>();
        if (!await db.Countries.AnyAsync(c => c.Code == d.CountryCode, ct))
        {
            errors.Add(new(K.Validation.NotFoundValue, new { field = K.Bank.CountryCode, value = d.CountryCode }));
        }

        if (await db.Banks.AnyAsync(b => b.Code == d.Code && b.Id != selfId, ct))
        {
            errors.Add(new(K.Bank.CodeExists, new { value = d.Code }));
        }

        return errors;
    }

    private static void ThrowIfInvalid(List<ValidationError> errors)
    {
        if (errors.Count > 0)
        {
            var first = errors[0];
            throw first.Key == K.Bank.CodeExists
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

    private static void MaxLength(List<ValidationError> errors, string? value, int max, string field)
    {
        if (value is not null && value.Length > max)
        {
            errors.Add(new(K.Validation.MaxLength, new { field, max }));
        }
    }

    private static void Apply(Bank b, BankSaveRequest d)
    {
        b.CountryCode = d.CountryCode;
        b.Code = d.Code;
        b.Bin = d.Bin;
        b.ShortName = d.ShortName;
        b.Name = d.Name;
        b.SwiftCode = d.SwiftCode;
        b.LogoUrl = d.LogoUrl;
        b.SupportsVietqr = d.SupportsVietqr;
        b.IsActive = d.IsActive;
    }

    #endregion
}
