// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã). Hỗ trợ cả mô hình 3 cấp cũ và 2 cấp sau sáp nhập 01/07/2025 nhờ effective_from/effective_to
/// </summary>
/// <remarks>Bảng <c>mdm.administrative_units</c>.</remarks>
public class AdministrativeUnit : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Quốc gia
    /// </summary>
    /// <remarks>FK → <c>mdm.countries(code)</c></remarks>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Đơn vị cấp trên (NULL nếu là cấp tỉnh)
    /// </summary>
    /// <remarks>FK → <c>mdm.administrative_units(id)</c></remarks>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Mã đơn vị hành chính (mã GSO/Tổng cục Thống kê)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên ngắn (VD: Hoàn Kiếm)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ (VD: Phường Hoàn Kiếm, Thành phố Hà Nội)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Tên tiếng Anh / không dấu
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Loại đơn vị: TP trực thuộc TW, tỉnh, quận, huyện, thị xã, phường, xã, thị trấn, đặc khu
    /// </summary>
    /// <remarks>Ràng buộc: <c>unit_type IN ('CENTRAL_CITY','PROVINCE','DISTRICT','URBAN_DISTRICT','TOWN','PROVINCIAL_CITY','WARD','COMMUNE','TOWNSHIP','SPECIAL_ZONE')</c></remarks>
    public string UnitType { get; set; } = string.Empty;

    /// <summary>
    /// Cấp: 1 = tỉnh, 2 = huyện (mô hình cũ) hoặc xã (mô hình 2 cấp), 3 = xã (mô hình cũ)
    /// </summary>
    /// <remarks>Ràng buộc: <c>level BETWEEN 1 AND 3</c></remarks>
    public short Level { get; set; }

    /// <summary>
    /// Đường dẫn mã từ gốc, VD: '01/00004' để truy vấn cây nhanh
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Mã vùng điện thoại cố định (024, 028...)
    /// </summary>
    public string? PhoneAreaCode { get; set; }

    /// <summary>
    /// Mã bưu chính
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Vĩ độ trung tâm
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Kinh độ trung tâm
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Ngày bắt đầu hiệu lực (VD: 2025-07-01 cho đơn vị sau sáp nhập)
    /// </summary>
    public DateOnly? EffectiveFrom { get; set; }

    /// <summary>
    /// Ngày hết hiệu lực (đơn vị đã giải thể/sáp nhập); NULL = còn hiệu lực
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Còn dùng để chọn trên form mới
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục quốc gia theo chuẩn ISO 3166 (qua <c>country_code</c>).
    /// </summary>
    public Country Country { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã) (qua <c>parent_id</c>).
    /// </summary>
    public AdministrativeUnit? Parent { get; set; }

    /// <summary>
    /// Danh sách AdministrativeUnit con (qua <c>mdm.administrative_units.parent_id</c>).
    /// </summary>
    public ICollection<AdministrativeUnit> Children { get; set; } = [];

    #endregion
}
