// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Đơn vị tính (Cái, Hộp, Thùng, Kg, Lít...). Quy đổi trong cùng nhóm qua ratio_to_base
/// </summary>
/// <remarks>Bảng <c>mdm.uoms</c>.</remarks>
public class Uom : SoftDeletableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = ĐVT chuẩn hệ thống)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Nhóm ĐVT
    /// </summary>
    /// <remarks>FK → <c>mdm.uom_categories(id)</c></remarks>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Mã ĐVT (PCS, BOX, CTN, KG, L)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị (Cái, Hộp, Thùng)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ký hiệu (kg, l, m²)
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Là đơn vị cơ sở của nhóm
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsBase { get; set; } = false;

    /// <summary>
    /// 1 đơn vị này = ratio_to_base đơn vị cơ sở (1 tấn = 1000 kg). Với nhóm COUNT, quy đổi theo sản phẩm do WMS quản lý
    /// </summary>
    /// <remarks>Ràng buộc: <c>ratio_to_base &gt; 0</c>; Mặc định DB: <c>1</c></remarks>
    public decimal RatioToBase { get; set; } = 1m;

    /// <summary>
    /// Số lẻ cho phép khi nhập số lượng
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short DecimalPlaces { get; set; } = 0;

    /// <summary>
    /// Tên ĐVT in trên hóa đơn điện tử
    /// </summary>
    public string? EinvoiceName { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Nhóm đơn vị tính (Số lượng, Khối lượng, Thể tích, Chiều dài, Diện tích, Thời gian) (qua <c>category_id</c>).
    /// </summary>
    public UomCategory Category { get; set; } = null!;

    #endregion
}
