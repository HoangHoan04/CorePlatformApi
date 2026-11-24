using System.Linq.Expressions;
using CorePlatformApi.Application.Common.Excel;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using K = CorePlatformApi.Application.Common.Localization.MessageKeys;

namespace CorePlatformApi.Application.Features.MasterData.AdministrativeUnitMappings;

/// <summary>
/// Ánh xạ đơn vị hành chính cũ → mới sau sáp nhập / chia tách / đổi tên (bảng administrative_unit_mappings),
/// dùng để chuyển đổi địa chỉ lịch sử. Theo mẫu CountryService; bảng không có trạng thái hoạt động nên
/// thay "kích hoạt / ngừng hoạt động" bằng xóa. Nhập Excel theo mã đơn vị cũ + mới.
/// </summary>
public interface IAdministrativeUnitMappingService
{
    Task<PagedResult<AdministrativeUnitMappingDto>> GetPagedAsync(AdministrativeUnitMappingQuery query, CancellationToken ct = default);
    Task<AdministrativeUnitMappingDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<AdministrativeUnitMappingDto> CreateAsync(AdministrativeUnitMappingSaveRequest request, CancellationToken ct = default);
    Task<AdministrativeUnitMappingDto> UpdateAsync(AdministrativeUnitMappingSaveRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<byte[]> GetTemplateAsync(CancellationToken ct = default);
    Task<byte[]> ExportAsync(AdministrativeUnitMappingQuery query, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default);
}

public sealed class AdministrativeUnitMappingService(
    IApplicationDbContext db,
    IExcelService excel,
    IAppLocalizer localizer) : IAdministrativeUnitMappingService
{
    private const int MaxExportRows = 100_000;

    #region Truy vấn

    public async Task<PagedResult<AdministrativeUnitMappingDto>> GetPagedAsync(AdministrativeUnitMappingQuery query, CancellationToken ct = default)
    {
        var q = Sort(Filter(query), query);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(ToDtoExpr).ToListAsync(ct);
        return new PagedResult<AdministrativeUnitMappingDto>(items, total, pageIndex, pageSize);
    }

    public async Task<AdministrativeUnitMappingDto> GetAsync(Guid id, CancellationToken ct = default) =>
        await db.AdministrativeUnitMappings.AsNoTracking().Where(m => m.Id == id).Select(ToDtoExpr).FirstOrDefaultAsync(ct)
        ?? throw AppException.NotFound(K.AdminUnitMapping.NotFound);

    private IQueryable<AdministrativeUnitMapping> Filter(AdministrativeUnitMappingQuery query)
    {
        var q = db.AdministrativeUnitMappings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var country = query.CountryCode.Trim().ToUpper();
            q = q.Where(m => m.OldUnit.CountryCode == country);
        }

        if (!string.IsNullOrWhiteSpace(query.MappingType))
        {
            q = q.Where(m => m.MappingType == query.MappingType);
        }

        if (query.UnitId.HasValue)
        {
            q = q.Where(m => m.OldUnitId == query.UnitId || m.NewUnitId == query.UnitId);
        }

        var keyword = (query.Keyword ?? query.Search)?.Trim().ToLower();
        if (!string.IsNullOrEmpty(keyword))
        {
            q = q.Where(m => m.OldUnit.Code.ToLower().Contains(keyword) || m.OldUnit.FullName.ToLower().Contains(keyword)
                             || m.NewUnit.Code.ToLower().Contains(keyword) || m.NewUnit.FullName.ToLower().Contains(keyword)
                             || (m.LegalReference != null && m.LegalReference.ToLower().Contains(keyword)));
        }

        if (query.FromDate.HasValue)
        {
            q = q.Where(m => m.EffectiveDate >= query.FromDate);
        }

        if (query.ToDate.HasValue)
        {
            q = q.Where(m => m.EffectiveDate <= query.ToDate);
        }

        return q;
    }

    private static IQueryable<AdministrativeUnitMapping> Sort(IQueryable<AdministrativeUnitMapping> q, AdministrativeUnitMappingQuery query)
    {
        var desc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortField?.ToLowerInvariant()) switch
        {
            "oldunitcode" => desc ? q.OrderByDescending(m => m.OldUnit.Code) : q.OrderBy(m => m.OldUnit.Code),
            "newunitcode" => desc ? q.OrderByDescending(m => m.NewUnit.Code) : q.OrderBy(m => m.NewUnit.Code),
            "mappingtype" => desc ? q.OrderByDescending(m => m.MappingType) : q.OrderBy(m => m.MappingType),
            "createdat" => desc ? q.OrderByDescending(m => m.CreatedAt) : q.OrderBy(m => m.CreatedAt),
            // Mặc định: mới nhất trước, rồi theo mã đơn vị mới
            _ => desc
                ? q.OrderBy(m => m.EffectiveDate).ThenBy(m => m.NewUnit.Code)
                : q.OrderByDescending(m => m.EffectiveDate).ThenBy(m => m.NewUnit.Code).ThenBy(m => m.OldUnit.Code),
        };
    }

    private static readonly Expression<Func<AdministrativeUnitMapping, AdministrativeUnitMappingDto>> ToDtoExpr = m =>
        new AdministrativeUnitMappingDto(
            m.Id, m.OldUnitId, m.OldUnit.Code, m.OldUnit.FullName, m.NewUnitId, m.NewUnit.Code, m.NewUnit.FullName,
            m.OldUnit.CountryCode, m.MappingType, m.EffectiveDate, m.LegalReference, m.Note, m.CreatedAt, m.UpdatedAt);

    #endregion

    #region Thêm / sửa / xóa

    public async Task<AdministrativeUnitMappingDto> CreateAsync(AdministrativeUnitMappingSaveRequest request, CancellationToken ct = default)
    {
        var data = Normalize(request);
        ThrowIfInvalid(await ValidateAsync(data, null, ct));

        var mapping = new AdministrativeUnitMapping();
        Apply(mapping, data);
        db.AdministrativeUnitMappings.Add(mapping);
        await db.SaveChangesAsync(ct);
        return await GetAsync(mapping.Id, ct);
    }

    public async Task<AdministrativeUnitMappingDto> UpdateAsync(AdministrativeUnitMappingSaveRequest request, CancellationToken ct = default)
    {
        var mapping = await FindAsync(request.Id ?? Guid.Empty, ct);
        var data = Normalize(request);
        ThrowIfInvalid(await ValidateAsync(data, mapping.Id, ct));

        Apply(mapping, data);
        await db.SaveChangesAsync(ct);
        return await GetAsync(mapping.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var mapping = await FindAsync(id, ct);
        db.AdministrativeUnitMappings.Remove(mapping);
        await db.SaveChangesAsync(ct);
    }

    private async Task<AdministrativeUnitMapping> FindAsync(Guid id, CancellationToken ct) =>
        await db.AdministrativeUnitMappings.FirstOrDefaultAsync(m => m.Id == id, ct)
        ?? throw AppException.NotFound(K.AdminUnitMapping.NotFound);

    #endregion

    #region Excel

    private static IReadOnlyList<ExcelColumn<AdministrativeUnitMappingDto>> ExcelColumns() =>
    [
        new(K.AdminUnitMapping.CountryCode, x => x.CountryCode, 12, Required: true, NumberFormat: "@"),
        new(K.AdminUnitMapping.OldUnitCode, x => x.OldUnitCode, 16, Required: true, NumberFormat: "@"),
        new(K.AdminUnitMapping.OldUnit, x => x.OldUnitName, 40),
        new(K.AdminUnitMapping.NewUnitCode, x => x.NewUnitCode, 16, Required: true, NumberFormat: "@"),
        new(K.AdminUnitMapping.NewUnit, x => x.NewUnitName, 40),
        new(K.AdminUnitMapping.MappingType, x => x.MappingType, 14, Required: true, ListValues: AdminUnitMappingTypes.All),
        new(K.AdminUnitMapping.EffectiveDate, x => x.EffectiveDate, 14, Required: true, NumberFormat: "dd/MM/yyyy"),
        new(K.AdminUnitMapping.LegalReference, x => x.LegalReference, 30),
        new(K.AdminUnitMapping.Note, x => x.Note, 40),
    ];

    public Task<byte[]> GetTemplateAsync(CancellationToken ct = default) =>
        excel.WriteAsync(K.AdminUnitMapping.EntityName, ExcelColumns(), [], ct);

    public async Task<byte[]> ExportAsync(AdministrativeUnitMappingQuery query, CancellationToken ct = default)
    {
        var rows = await Sort(Filter(query), query).Take(MaxExportRows).Select(ToDtoExpr).ToListAsync(ct);
        return await excel.WriteAsync(K.AdminUnitMapping.EntityName, ExcelColumns(), rows, ct);
    }

    /// <summary>
    /// Nhập theo (mã đơn vị cũ, mã đơn vị mới) trong một quốc gia: chưa có -> thêm, đã có -> cập nhật.
    /// Cột tên đơn vị chỉ để người dùng đối chiếu, khi nhập bị bỏ qua.
    /// </summary>
    public async Task<ImportResult> ImportAsync(Stream file, CancellationToken ct = default)
    {
        var rows = excel.Read(file, ExcelColumns().Count);

        var units = await db.AdministrativeUnits.AsNoTracking()
            .Select(u => new { u.Id, u.CountryCode, u.Code })
            .ToListAsync(ct);
        var unitByKey = units.ToDictionary(u => $"{u.CountryCode}|{u.Code}", u => u.Id, StringComparer.OrdinalIgnoreCase);
        var existing = await db.AdministrativeUnitMappings.ToListAsync(ct);
        var byPair = existing.ToDictionary(m => (m.OldUnitId, m.NewUnitId));
        var seen = new Dictionary<(Guid, Guid), int>();

        var messages = new List<string>();
        int created = 0, updated = 0;
        foreach (var row in rows)
        {
            var errors = new List<ValidationError>();
            var country = row[0].Trim().ToUpperInvariant();
            Required(errors, country, K.AdminUnitMapping.CountryCode);

            var oldId = ResolveUnit(row[1], K.AdminUnitMapping.OldUnitCode);
            var newId = ResolveUnit(row[3], K.AdminUnitMapping.NewUnitCode);

            if (!ExcelValues.TryParseDate(row[6], out var date))
            {
                errors.Add(new(K.Validation.InvalidDate, new { field = K.AdminUnitMapping.EffectiveDate, value = row[6] }));
            }

            var data = Normalize(new AdministrativeUnitMappingSaveRequest
            {
                OldUnitId = oldId ?? Guid.Empty,
                NewUnitId = newId ?? Guid.Empty,
                MappingType = row[5],
                EffectiveDate = date,
                LegalReference = row[7],
                Note = row[8],
            });
            errors.AddRange(Validate(data, oldId.HasValue && newId.HasValue));

            if (oldId.HasValue && newId.HasValue && !seen.TryAdd((oldId.Value, newId.Value), row.RowNumber))
            {
                errors.Add(new(K.Import.DuplicateInFile,
                    new { field = K.AdminUnitMapping.NewUnitCode, value = row[3], other = seen[(oldId.Value, newId.Value)] }));
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

            if (!byPair.TryGetValue((data.OldUnitId, data.NewUnitId), out var mapping))
            {
                mapping = new AdministrativeUnitMapping();
                db.AdministrativeUnitMappings.Add(mapping);
                byPair[(data.OldUnitId, data.NewUnitId)] = mapping;
                created++;
            }
            else
            {
                updated++;
            }

            Apply(mapping, data);
            continue;

            Guid? ResolveUnit(string code, string field)
            {
                var c = code.Trim().ToUpperInvariant();
                if (c.Length == 0)
                {
                    errors.Add(new(K.Validation.Required, new { field }));
                    return null;
                }

                if (unitByKey.TryGetValue($"{country}|{c}", out var id))
                {
                    return id;
                }

                errors.Add(new(K.Validation.NotFoundValue, new { field, value = c }));
                return null;
            }
        }

        await db.SaveChangesAsync(ct);
        var success = created + updated;
        return new ImportResult(rows.Count, success, rows.Count - success, messages)
        {
            CreatedCount = created,
            UpdatedCount = updated,
        };
    }

    #endregion

    #region Chuẩn hóa + kiểm tra dữ liệu

    private static AdministrativeUnitMappingSaveRequest Normalize(AdministrativeUnitMappingSaveRequest r) => new()
    {
        Id = r.Id,
        OldUnitId = r.OldUnitId,
        NewUnitId = r.NewUnitId,
        MappingType = (r.MappingType ?? string.Empty).Trim().ToUpperInvariant(),
        EffectiveDate = r.EffectiveDate,
        LegalReference = ExcelValues.NullIfEmpty(r.LegalReference),
        Note = ExcelValues.NullIfEmpty(r.Note),
    };

    /// <summary>
    /// Kiểm tra không cần database (dùng chung form + nhập Excel).
    /// </summary>
    private static List<ValidationError> Validate(AdministrativeUnitMappingSaveRequest d, bool unitsResolved = true)
    {
        var errors = new List<ValidationError>();
        if (unitsResolved)
        {
            if (d.OldUnitId == Guid.Empty)
            {
                errors.Add(new(K.Validation.Required, new { field = K.AdminUnitMapping.OldUnit }));
            }

            if (d.NewUnitId == Guid.Empty)
            {
                errors.Add(new(K.Validation.Required, new { field = K.AdminUnitMapping.NewUnit }));
            }

            if (d.OldUnitId != Guid.Empty && d.OldUnitId == d.NewUnitId)
            {
                errors.Add(new(K.AdminUnitMapping.SameUnit));
            }
        }

        Required(errors, d.MappingType, K.AdminUnitMapping.MappingType);
        if (d.MappingType.Length > 0 && !AdminUnitMappingTypes.All.Contains(d.MappingType))
        {
            errors.Add(new(K.Validation.InvalidValue, new { field = K.AdminUnitMapping.MappingType, value = d.MappingType }));
        }

        if (!d.EffectiveDate.HasValue)
        {
            errors.Add(new(K.Validation.Required, new { field = K.AdminUnitMapping.EffectiveDate }));
        }

        if (d.LegalReference is { Length: > 255 })
        {
            errors.Add(new(K.Validation.MaxLength, new { field = K.AdminUnitMapping.LegalReference, max = 255 }));
        }

        return errors;
    }

    /// <summary>
    /// Kiểm tra đầy đủ cho form: đơn vị tồn tại, cùng quốc gia, cặp cũ → mới chưa khai báo.
    /// </summary>
    private async Task<List<ValidationError>> ValidateAsync(AdministrativeUnitMappingSaveRequest d, Guid? selfId, CancellationToken ct)
    {
        var errors = Validate(d);
        if (errors.Count > 0)
        {
            return errors;
        }

        var units = await db.AdministrativeUnits.AsNoTracking()
            .Where(u => u.Id == d.OldUnitId || u.Id == d.NewUnitId)
            .Select(u => new { u.Id, u.CountryCode })
            .ToListAsync(ct);
        var oldUnit = units.FirstOrDefault(u => u.Id == d.OldUnitId);
        var newUnit = units.FirstOrDefault(u => u.Id == d.NewUnitId);
        if (oldUnit is null)
        {
            errors.Add(new(K.Validation.NotFoundValue, new { field = K.AdminUnitMapping.OldUnit, value = d.OldUnitId }));
        }

        if (newUnit is null)
        {
            errors.Add(new(K.Validation.NotFoundValue, new { field = K.AdminUnitMapping.NewUnit, value = d.NewUnitId }));
        }

        if (oldUnit is not null && newUnit is not null && oldUnit.CountryCode != newUnit.CountryCode)
        {
            errors.Add(new(K.AdminUnitMapping.OtherCountry));
        }

        if (await db.AdministrativeUnitMappings.AnyAsync(
                m => m.OldUnitId == d.OldUnitId && m.NewUnitId == d.NewUnitId && m.Id != selfId, ct))
        {
            errors.Add(new(K.AdminUnitMapping.Exists));
        }

        return errors;
    }

    private static void ThrowIfInvalid(List<ValidationError> errors)
    {
        if (errors.Count > 0)
        {
            var first = errors[0];
            throw first.Key == K.AdminUnitMapping.Exists
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

    private static void Apply(AdministrativeUnitMapping m, AdministrativeUnitMappingSaveRequest d)
    {
        m.OldUnitId = d.OldUnitId;
        m.NewUnitId = d.NewUnitId;
        m.MappingType = d.MappingType;
        m.EffectiveDate = d.EffectiveDate!.Value;
        m.LegalReference = d.LegalReference;
        m.Note = d.Note;
    }

    #endregion
}
