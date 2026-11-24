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

namespace CorePlatformApi.Application.Features.MasterData.AdministrativeUnits;

/// <summary>
/// Đơn vị hành chính phân cấp (bảng administrative_units): Tỉnh/TP → [Quận/Huyện] → Phường/Xã.
/// Theo mẫu CountryService; khác biệt: cấp (level) + đường dẫn cây (path) tự tính từ đơn vị cấp trên,
/// nhập Excel theo mã đơn vị cấp trên (thứ tự dòng trong file tùy ý).
/// </summary>
public interface IAdministrativeUnitService
{
    Task<PagedResult<AdministrativeUnitDto>> GetPagedAsync(AdministrativeUnitQuery query, CancellationToken ct = default);
    Task<AdministrativeUnitDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<AdministrativeUnitDto> CreateAsync(AdministrativeUnitSaveRequest request, CancellationToken ct = default);
    Task<AdministrativeUnitDto> UpdateAsync(AdministrativeUnitSaveRequest request, CancellationToken ct = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(AdministrativeUnitSelectBoxQuery query, CancellationToken ct = default);
    Task<byte[]> GetTemplateAsync(CancellationToken ct = default);
    Task<byte[]> ExportAsync(AdministrativeUnitQuery query, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default);
}

public sealed partial class AdministrativeUnitService(
    IApplicationDbContext db,
    IExcelService excel,
    IAppLocalizer localizer) : IAdministrativeUnitService
{
    private const int MaxExportRows = 100_000;

    [GeneratedRegex("^[A-Za-z0-9_.-]{1,20}$")]
    private static partial Regex CodeRegex();

    [GeneratedRegex("^[0-9 +-]{1,10}$")]
    private static partial Regex PhoneRegex();

    #region Truy vấn

    public async Task<PagedResult<AdministrativeUnitDto>> GetPagedAsync(AdministrativeUnitQuery query, CancellationToken ct = default)
    {
        var q = Sort(Filter(query), query);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(ToDtoExpr).ToListAsync(ct);
        return new PagedResult<AdministrativeUnitDto>(items, total, pageIndex, pageSize);
    }

    public async Task<AdministrativeUnitDto> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.AdministrativeUnits.AsNoTracking().Where(u => u.Id == id).Select(ToDtoExpr).FirstOrDefaultAsync(ct)
        ?? throw AppException.NotFound(K.AdminUnit.NotFound);

    /// <summary>
    /// Ô chọn theo tầng: chọn tỉnh (Level=1) rồi chọn xã (ParentId = tỉnh). Tên hiển thị là tên đầy đủ để phân biệt trùng tên.
    /// </summary>
    public async Task<IReadOnlyList<SelectBoxItem>> GetSelectBoxAsync(AdministrativeUnitSelectBoxQuery query, CancellationToken ct = default)
    {
        var q = db.AdministrativeUnits.AsNoTracking();
        if (!query.IncludeInactive)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            q = q.Where(u => u.IsActive && (u.EffectiveTo == null || u.EffectiveTo > today));
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var country = query.CountryCode.Trim().ToUpper();
            q = q.Where(u => u.CountryCode == country);
        }

        if (query.ParentId.HasValue)
        {
            q = q.Where(u => u.ParentId == query.ParentId);
        }

        if (query.Level.HasValue)
        {
            q = q.Where(u => u.Level == query.Level);
        }

        if (query.MaxLevel.HasValue)
        {
            q = q.Where(u => u.Level <= query.MaxLevel);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(u => u.Code.ToLower().Contains(term) || u.FullName.ToLower().Contains(term)
                             || (u.NameEn != null && u.NameEn.ToLower().Contains(term)));
        }

        return await q.OrderBy(u => u.Level).ThenBy(u => u.SortOrder).ThenBy(u => u.Name)
            .Take(Math.Clamp(query.Take, 1, 1000))
            .Select(u => new SelectBoxItem(u.Id, u.Code, u.FullName))
            .ToListAsync(ct);
    }

    private IQueryable<AdministrativeUnit> Filter(AdministrativeUnitQuery query)
    {
        var q = db.AdministrativeUnits.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var country = query.CountryCode.Trim().ToUpper();
            q = q.Where(u => u.CountryCode == country);
        }

        if (query.ParentId.HasValue)
        {
            q = q.Where(u => u.ParentId == query.ParentId);
        }

        if (query.Level.HasValue)
        {
            q = q.Where(u => u.Level == query.Level);
        }

        if (!string.IsNullOrWhiteSpace(query.UnitType))
        {
            q = q.Where(u => u.UnitType == query.UnitType);
        }

        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var code = query.Code.Trim().ToLower();
            q = q.Where(u => u.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            q = q.Where(u => u.Name.ToLower().Contains(name) || u.FullName.ToLower().Contains(name)
                             || (u.NameEn != null && u.NameEn.ToLower().Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(u => u.Code.ToLower().Contains(term) || u.FullName.ToLower().Contains(term));
        }

        if (query.IsActive.HasValue)
        {
            q = q.Where(u => u.IsActive == query.IsActive.Value);
        }

        if (query.IsEffective.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            q = query.IsEffective.Value
                ? q.Where(u => u.EffectiveTo == null || u.EffectiveTo > today)
                : q.Where(u => u.EffectiveTo != null && u.EffectiveTo <= today);
        }

        return q;
    }

    private static IQueryable<AdministrativeUnit> Sort(IQueryable<AdministrativeUnit> q, AdministrativeUnitQuery query)
    {
        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortField?.ToLowerInvariant()) switch
        {
            "code" => desc ? q.OrderByDescending(u => u.Code) : q.OrderBy(u => u.Code),
            "name" => desc ? q.OrderByDescending(u => u.Name) : q.OrderBy(u => u.Name),
            "fullname" => desc ? q.OrderByDescending(u => u.FullName) : q.OrderBy(u => u.FullName),
            "level" => desc ? q.OrderByDescending(u => u.Level).ThenBy(u => u.Code) : q.OrderBy(u => u.Level).ThenBy(u => u.Code),
            "unittype" => desc ? q.OrderByDescending(u => u.UnitType) : q.OrderBy(u => u.UnitType),
            "effectivefrom" => desc ? q.OrderByDescending(u => u.EffectiveFrom) : q.OrderBy(u => u.EffectiveFrom),
            "isactive" => desc ? q.OrderByDescending(u => u.IsActive) : q.OrderBy(u => u.IsActive),
            "createdat" => desc ? q.OrderByDescending(u => u.CreatedAt) : q.OrderBy(u => u.CreatedAt),
            // Mặc định: theo cây (path) để cấp trên đứng trước cấp dưới
            _ => desc
                ? q.OrderByDescending(u => u.Path ?? u.Code)
                : q.OrderBy(u => u.CountryCode).ThenBy(u => u.Path ?? u.Code),
        };
    }

    private static readonly Expression<Func<AdministrativeUnit, AdministrativeUnitDto>> ToDtoExpr = u => new AdministrativeUnitDto(
        u.Id, u.CountryCode, u.ParentId,
        u.Parent != null ? u.Parent.Code : null,
        u.Parent != null ? u.Parent.FullName : null,
        u.Code, u.Name, u.FullName, u.NameEn, u.UnitType, u.Level, u.Path, u.PhoneAreaCode, u.PostalCode,
        u.Latitude, u.Longitude, u.EffectiveFrom, u.EffectiveTo, u.SortOrder, u.IsActive,
        u.Children.Count(), u.CreatedAt, u.UpdatedAt);

    #endregion

    #region Thêm / sửa / trạng thái

    public async Task<AdministrativeUnitDto> CreateAsync(AdministrativeUnitSaveRequest request, CancellationToken ct = default)
    {
        var data = Normalize(request);
        ThrowIfInvalid(Validate(data));

        var errors = new List<ValidationError>();
        if (!await db.Countries.AnyAsync(c => c.Code == data.CountryCode, ct))
        {
            errors.Add(new(K.Validation.NotFoundValue, new { field = K.AdminUnit.CountryCode, value = data.CountryCode }));
        }

        AdministrativeUnit? parent = null;
        if (data.ParentId.HasValue)
        {
            parent = await db.AdministrativeUnits.AsNoTracking().FirstOrDefaultAsync(u => u.Id == data.ParentId, ct);
            errors.AddRange(ValidateParent(parent, data.CountryCode));
        }

        if (await db.AdministrativeUnits.AnyAsync(u => u.CountryCode == data.CountryCode && u.Code == data.Code, ct))
        {
            errors.Add(new(K.AdminUnit.CodeExists, new { value = data.Code }));
        }

        ThrowIfInvalid(errors);

        var unit = new AdministrativeUnit { CountryCode = data.CountryCode, Code = data.Code };
        SetHierarchy(unit, parent);
        Apply(unit, data, parent);
        db.AdministrativeUnits.Add(unit);
        await db.SaveChangesAsync(ct);
        return await GetAsync(unit.Id, ct);
    }

    public async Task<AdministrativeUnitDto> UpdateAsync(AdministrativeUnitSaveRequest request, CancellationToken ct = default)
    {
        var unit = await db.AdministrativeUnits.FirstOrDefaultAsync(u => u.Id == (request.Id ?? Guid.Empty), ct)
                   ?? throw AppException.NotFound(K.AdminUnit.NotFound);

        // Quốc gia, cấp trên, mã giữ nguyên (cấp + đường dẫn cây phụ thuộc vào chúng)
        var data = Normalize(request);
        data.CountryCode = unit.CountryCode;
        data.ParentId = unit.ParentId;
        data.Code = unit.Code;
        ThrowIfInvalid(Validate(data));

        var parent = unit.ParentId.HasValue
            ? await db.AdministrativeUnits.AsNoTracking().FirstOrDefaultAsync(u => u.Id == unit.ParentId, ct)
            : null;
        Apply(unit, data, parent);
        await db.SaveChangesAsync(ct);
        return await GetAsync(unit.Id, ct);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var unit = await db.AdministrativeUnits.FirstOrDefaultAsync(u => u.Id == id, ct)
                   ?? throw AppException.NotFound(K.AdminUnit.NotFound);
        if (unit.IsActive == isActive)
        {
            return;
        }

        unit.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    #endregion

    #region Excel

    private static IReadOnlyList<ExcelColumn<AdministrativeUnitDto>> ExcelColumns(string yes, string no) =>
    [
        new(K.AdminUnit.CountryCode, x => x.CountryCode, 12, Required: true, NumberFormat: "@"),
        new(K.AdminUnit.ParentCode, x => x.ParentCode, 16, NumberFormat: "@"),
        new(K.AdminUnit.Code, x => x.Code, 14, Required: true, NumberFormat: "@"),
        new(K.AdminUnit.Name, x => x.Name, 26, Required: true),
        new(K.AdminUnit.FullName, x => x.FullName, 42),
        new(K.AdminUnit.NameEn, x => x.NameEn, 26),
        new(K.AdminUnit.UnitType, x => x.UnitType, 18, Required: true, ListValues: AdminUnitTypes.All),
        new(K.AdminUnit.PhoneAreaCode, x => x.PhoneAreaCode, 12, NumberFormat: "@"),
        new(K.AdminUnit.PostalCode, x => x.PostalCode, 12, NumberFormat: "@"),
        new(K.AdminUnit.Latitude, x => x.Latitude, 13, NumberFormat: "0.0000000"),
        new(K.AdminUnit.Longitude, x => x.Longitude, 13, NumberFormat: "0.0000000"),
        new(K.AdminUnit.EffectiveFrom, x => x.EffectiveFrom, 14, NumberFormat: "dd/MM/yyyy"),
        new(K.AdminUnit.EffectiveTo, x => x.EffectiveTo, 14, NumberFormat: "dd/MM/yyyy"),
        new(K.AdminUnit.SortOrder, x => x.SortOrder, 10, NumberFormat: "0"),
        new(K.AdminUnit.IsActive, x => x.IsActive ? yes : no, 12, ListValues: [yes, no]),
    ];

    public async Task<byte[]> GetTemplateAsync(CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        return await excel.WriteAsync(K.AdminUnit.EntityName, ExcelColumns(yes, no), [], ct);
    }

    public async Task<byte[]> ExportAsync(AdministrativeUnitQuery query, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = await Sort(Filter(query), query).Take(MaxExportRows).Select(ToDtoExpr).ToListAsync(ct);
        return await excel.WriteAsync(K.AdminUnit.EntityName, ExcelColumns(yes, no), rows, ct);
    }

    /// <summary>
    /// Nhập theo (quốc gia, mã): chưa có -> thêm mới, đã có -> cập nhật. Đơn vị cấp trên tìm theo mã trong cùng
    /// quốc gia — có thể nằm trong database hoặc ở bất kỳ dòng nào của file (xử lý nhiều lượt đến khi hết dòng giải được).
    /// </summary>
    public async Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var (yes, no) = await YesNoAsync(ct);
        var rows = excel.Read(file, ExcelColumns(yes, no).Count);

        var countries = (await db.Countries.AsNoTracking().Select(c => c.Code).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var units = await db.AdministrativeUnits.ToListAsync(ct);
        var byKey = units.ToDictionary(u => Key(u.CountryCode, u.Code), StringComparer.OrdinalIgnoreCase);

        var rowErrors = new Dictionary<int, List<ValidationError>>();
        var pending = new List<(ExcelRow Row, AdministrativeUnitSaveRequest Data, string? ParentCode)>();
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Lượt 1: đọc + kiểm tra từng dòng độc lập
        foreach (var row in rows)
        {
            var errors = new List<ValidationError>();
            if (!ExcelValues.TryParseDecimal(row[9], out var lat))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.AdminUnit.Latitude }));
            }

            if (!ExcelValues.TryParseDecimal(row[10], out var lng))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.AdminUnit.Longitude }));
            }

            if (!ExcelValues.TryParseDate(row[11], out var from))
            {
                errors.Add(new(K.Validation.InvalidDate, new { field = K.AdminUnit.EffectiveFrom, value = row[11] }));
            }

            if (!ExcelValues.TryParseDate(row[12], out var to))
            {
                errors.Add(new(K.Validation.InvalidDate, new { field = K.AdminUnit.EffectiveTo, value = row[12] }));
            }

            if (!ExcelValues.TryParseInt(row[13], 0, out var sortOrder))
            {
                errors.Add(new(K.Validation.InvalidNumber, new { field = K.AdminUnit.SortOrder }));
            }

            if (!ExcelValues.TryParseBool(row[14], yes, no, true, out var isActive))
            {
                errors.Add(new(K.Validation.InvalidBoolean, new { field = K.AdminUnit.IsActive, yes, no }));
            }

            var data = Normalize(new AdministrativeUnitSaveRequest
            {
                CountryCode = row[0],
                Code = row[2],
                Name = row[3],
                FullName = row[4],
                NameEn = row[5],
                UnitType = row[6],
                PhoneAreaCode = row[7],
                PostalCode = row[8],
                Latitude = lat,
                Longitude = lng,
                EffectiveFrom = from,
                EffectiveTo = to,
                SortOrder = sortOrder,
                IsActive = isActive,
            });
            errors.AddRange(Validate(data));

            if (data.CountryCode.Length > 0 && !countries.Contains(data.CountryCode))
            {
                errors.Add(new(K.Validation.NotFoundValue, new { field = K.AdminUnit.CountryCode, value = data.CountryCode }));
            }

            var key = Key(data.CountryCode, data.Code);
            if (data.Code.Length > 0 && !seen.TryAdd(key, row.RowNumber))
            {
                errors.Add(new(K.Import.DuplicateInFile, new { field = K.AdminUnit.Code, value = data.Code, other = seen[key] }));
            }

            if (errors.Count > 0)
            {
                rowErrors[row.RowNumber] = errors;
            }
            else
            {
                pending.Add((row, data, ExcelValues.NullIfEmpty(row[1])?.ToUpperInvariant()));
            }
        }

        // Lượt 2..n: giải đơn vị cấp trên (DB hoặc dòng đã xử lý), lặp đến khi không còn tiến triển
        int created = 0, updated = 0;
        bool progressed;
        do
        {
            progressed = false;
            foreach (var item in pending.ToList())
            {
                var (row, data, parentCode) = item;
                AdministrativeUnit? parent = null;
                if (parentCode is not null)
                {
                    var parentKey = Key(data.CountryCode, parentCode);
                    if (!byKey.TryGetValue(parentKey, out parent))
                    {
                        // Có thể cấp trên nằm ở dòng phía sau -> chờ lượt sau
                        if (pending.Any(p => Key(p.Data.CountryCode, p.Data.Code).Equals(parentKey, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        Fail(row, new(K.AdminUnit.ParentNotFound, new { value = parentCode }));
                        pending.Remove(item);
                        continue;
                    }
                }

                List<ValidationError> errors = parent is null ? [] : ValidateParent(parent, data.CountryCode);
                byKey.TryGetValue(Key(data.CountryCode, data.Code), out var unit);
                if (unit is not null && unit.ParentId != parent?.Id)
                {
                    errors.Add(new(K.AdminUnit.ParentImmutable, new { value = data.Code }));
                }

                pending.Remove(item);
                progressed = true;
                if (errors.Count > 0)
                {
                    rowErrors[row.RowNumber] = errors;
                    continue;
                }

                if (unit is null)
                {
                    unit = new AdministrativeUnit { CountryCode = data.CountryCode, Code = data.Code };
                    SetHierarchy(unit, parent);
                    db.AdministrativeUnits.Add(unit);
                    byKey[Key(unit.CountryCode, unit.Code)] = unit;
                    created++;
                }
                else
                {
                    updated++;
                }

                Apply(unit, data, parent);
            }
        }
        while (progressed && pending.Count > 0);

        // Còn lại = vòng tham chiếu cấp trên trong file
        foreach (var (row, data, parentCode) in pending)
        {
            Fail(row, new(K.AdminUnit.ParentNotFound, new { value = parentCode }));
        }

        await db.SaveChangesAsync(ct);

        var messages = new List<string>();
        foreach (var (rowNumber, errors) in rowErrors.OrderBy(e => e.Key))
        {
            foreach (var e in errors)
            {
                messages.Add(await localizer.GetAsync(K.Import.RowError,
                    new { row = rowNumber, message = await localizer.GetAsync(e.Key, e.Args, ct) }, ct));
            }
        }

        var success = created + updated;
        return new ImportResult(rows.Count, success, rows.Count - success, messages)
        {
            CreatedCount = created,
            UpdatedCount = updated,
        };

        void Fail(ExcelRow row, ValidationError error)
        {
            if (!rowErrors.TryGetValue(row.RowNumber, out var list))
            {
                rowErrors[row.RowNumber] = list = [];
            }

            list.Add(error);
        }
    }

    private static string Key(string countryCode, string code) => $"{countryCode}|{code}";

    private async Task<(string Yes, string No)> YesNoAsync(CancellationToken ct) =>
        (await localizer.GetAsync(K.Common.Yes, ct: ct), await localizer.GetAsync(K.Common.No, ct: ct));

    #endregion

    #region Chuẩn hóa + kiểm tra dữ liệu

    private static AdministrativeUnitSaveRequest Normalize(AdministrativeUnitSaveRequest r) => new()
    {
        Id = r.Id,
        CountryCode = (r.CountryCode ?? string.Empty).Trim().ToUpperInvariant(),
        ParentId = r.ParentId,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        FullName = ExcelValues.NullIfEmpty(r.FullName),
        NameEn = ExcelValues.NullIfEmpty(r.NameEn),
        UnitType = (r.UnitType ?? string.Empty).Trim().ToUpperInvariant(),
        PhoneAreaCode = ExcelValues.NullIfEmpty(r.PhoneAreaCode),
        PostalCode = ExcelValues.NullIfEmpty(r.PostalCode),
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        EffectiveFrom = r.EffectiveFrom,
        EffectiveTo = r.EffectiveTo,
        SortOrder = r.SortOrder,
        IsActive = r.IsActive,
    };

    /// <summary>
    /// Kiểm tra định dạng / bắt buộc / độ dài / khoảng giá trị — không truy vấn database.
    /// </summary>
    private static List<ValidationError> Validate(AdministrativeUnitSaveRequest d)
    {
        var errors = new List<ValidationError>();
        Required(errors, d.CountryCode, K.AdminUnit.CountryCode);
        Required(errors, d.Code, K.AdminUnit.Code);
        Required(errors, d.Name, K.AdminUnit.Name);
        Required(errors, d.UnitType, K.AdminUnit.UnitType);

        if (d.Code.Length > 0 && !CodeRegex().IsMatch(d.Code))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.AdminUnit.Code, value = d.Code }));
        }

        if (d.UnitType.Length > 0 && !AdminUnitTypes.All.Contains(d.UnitType))
        {
            errors.Add(new(K.Validation.InvalidValue, new { field = K.AdminUnit.UnitType, value = d.UnitType }));
        }

        if (d.PhoneAreaCode is not null && !PhoneRegex().IsMatch(d.PhoneAreaCode))
        {
            errors.Add(new(K.Validation.InvalidFormat, new { field = K.AdminUnit.PhoneAreaCode, value = d.PhoneAreaCode }));
        }

        MaxLength(errors, d.Name, 150, K.AdminUnit.Name);
        MaxLength(errors, d.FullName, 255, K.AdminUnit.FullName);
        MaxLength(errors, d.NameEn, 150, K.AdminUnit.NameEn);
        MaxLength(errors, d.PostalCode, 10, K.AdminUnit.PostalCode);

        if (d.Latitude is < -90 or > 90)
        {
            errors.Add(new(K.Validation.OutOfRange, new { field = K.AdminUnit.Latitude, min = -90, max = 90 }));
        }

        if (d.Longitude is < -180 or > 180)
        {
            errors.Add(new(K.Validation.OutOfRange, new { field = K.AdminUnit.Longitude, min = -180, max = 180 }));
        }

        if (d.EffectiveFrom.HasValue && d.EffectiveTo.HasValue && d.EffectiveTo <= d.EffectiveFrom)
        {
            errors.Add(new(K.Validation.DateRange, new { from = K.AdminUnit.EffectiveFrom, to = K.AdminUnit.EffectiveTo }));
        }

        return errors;
    }

    private static List<ValidationError> ValidateParent(AdministrativeUnit? parent, string countryCode)
    {
        var errors = new List<ValidationError>();
        if (parent is null)
        {
            errors.Add(new(K.AdminUnit.ParentNotFound, new { value = string.Empty }));
        }
        else if (!parent.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new(K.AdminUnit.ParentOtherCountry, new { value = parent.Code }));
        }
        else if (parent.Level >= AdminUnitTypes.MaxLevel)
        {
            errors.Add(new(K.AdminUnit.MaxLevelExceeded, new { max = AdminUnitTypes.MaxLevel }));
        }

        return errors;
    }

    private static void ThrowIfInvalid(List<ValidationError> errors)
    {
        if (errors.Count > 0)
        {
            var first = errors[0];
            throw first.Key == K.AdminUnit.CodeExists
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

    /// <summary>
    /// Cấp + đường dẫn cây theo đơn vị cấp trên (chỉ đặt khi tạo mới).
    /// </summary>
    private static void SetHierarchy(AdministrativeUnit unit, AdministrativeUnit? parent)
    {
        unit.ParentId = parent?.Id;
        unit.Level = (short)(parent is null ? 1 : parent.Level + 1);
        unit.Path = parent is null ? unit.Code : $"{parent.Path ?? parent.Code}/{unit.Code}";
    }

    private static void Apply(AdministrativeUnit u, AdministrativeUnitSaveRequest d, AdministrativeUnit? parent)
    {
        u.Name = d.Name;
        u.FullName = d.FullName ?? (parent is null ? d.Name : $"{d.Name}, {parent.FullName}");
        u.NameEn = d.NameEn;
        u.UnitType = d.UnitType;
        u.PhoneAreaCode = d.PhoneAreaCode;
        u.PostalCode = d.PostalCode;
        u.Latitude = d.Latitude;
        u.Longitude = d.Longitude;
        u.EffectiveFrom = d.EffectiveFrom;
        u.EffectiveTo = d.EffectiveTo;
        u.SortOrder = d.SortOrder;
        u.IsActive = d.IsActive;
    }

    #endregion
}
