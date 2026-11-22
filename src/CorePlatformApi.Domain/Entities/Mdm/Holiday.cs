// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Ngày lễ quốc gia (tenant_id NULL) và ngày lễ/nghỉ riêng của tenant
/// </summary>
/// <remarks>Bảng <c>mdm.holidays</c>.</remarks>
public class Holiday : SoftDeletableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = lễ quốc gia)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Quốc gia
    /// </summary>
    /// <remarks>Mặc định DB: <c>'VN'</c>; FK → <c>mdm.countries(code)</c></remarks>
    public string CountryCode { get; set; } = "VN";

    /// <summary>
    /// Mã ngày lễ (TET, NATIONAL_DAY, LABOR_DAY)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Tên ngày lễ
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ngày dương lịch cụ thể (đã quy đổi nếu là âm lịch)
    /// </summary>
    public DateOnly HolidayDate { get; set; }

    /// <summary>
    /// Năm (tính tự động để lọc nhanh)
    /// </summary>
    /// <remarks>Cột tính toán (STORED): <c>EXTRACT(YEAR FROM holiday_date)::smallint</c></remarks>
    public short? Year { get; set; }

    /// <summary>
    /// Gốc là ngày âm lịch (Tết, Giỗ Tổ)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsLunar { get; set; } = false;

    /// <summary>
    /// Ngày âm
    /// </summary>
    public short? LunarDay { get; set; }

    /// <summary>
    /// Tháng âm
    /// </summary>
    public short? LunarMonth { get; set; }

    /// <summary>
    /// Loại: lễ chính thức, nghỉ bù, nghỉ công ty, ngày kỷ niệm (không nghỉ)
    /// </summary>
    /// <remarks>Ràng buộc: <c>holiday_type IN ('PUBLIC','COMPENSATORY','COMPANY','OBSERVANCE')</c>; Mặc định DB: <c>'PUBLIC'</c></remarks>
    public string HolidayType { get; set; } = "PUBLIC";

    /// <summary>
    /// Có được nghỉ không
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsDayOff { get; set; } = true;

    /// <summary>
    /// Nghỉ hưởng lương (HRM tính công)
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsPaid { get; set; } = true;

    /// <summary>
    /// Hệ số lương nếu đi làm ngày này (BLLĐ: tối thiểu 300%)
    /// </summary>
    /// <remarks>Mặc định DB: <c>3.00</c></remarks>
    public decimal? PayMultiplier { get; set; } = 3.00m;

    /// <summary>
    /// Văn bản quy định (thông báo của Bộ LĐ)
    /// </summary>
    public string? LegalReference { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Note { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Danh mục quốc gia theo chuẩn ISO 3166 (qua <c>country_code</c>).
    /// </summary>
    public Country Country { get; set; } = null!;

    #endregion
}
