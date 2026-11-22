// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Bảng quy đổi ĐVT tổng quát giữa hai đơn vị (kể cả khác nhóm/không tỷ lệ cố định). Quy đổi riêng theo sản phẩm (1 thùng = 24 lon) đặt ở WMS
/// </summary>
/// <remarks>Bảng <c>mdm.uom_conversions</c>.</remarks>
public class UomConversion : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = chuẩn hệ thống)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Đơn vị nguồn
    /// </summary>
    /// <remarks>FK → <c>mdm.uoms(id)</c></remarks>
    public Guid FromUomId { get; set; }

    /// <summary>
    /// Đơn vị đích
    /// </summary>
    /// <remarks>FK → <c>mdm.uoms(id)</c></remarks>
    public Guid ToUomId { get; set; }

    /// <summary>
    /// 1 from = factor to
    /// </summary>
    /// <remarks>Ràng buộc: <c>factor &gt; 0</c></remarks>
    public decimal Factor { get; set; }

    /// <summary>
    /// Kiểu làm tròn
    /// </summary>
    /// <remarks>Ràng buộc: <c>rounding_mode IN ('HALF_UP','UP','DOWN','NONE')</c>; Mặc định DB: <c>'HALF_UP'</c></remarks>
    public string RoundingMode { get; set; } = "HALF_UP";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Đơn vị tính (Cái, Hộp, Thùng, Kg, Lít (qua <c>from_uom_id</c>).
    /// </summary>
    public Uom FromUom { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Đơn vị tính (Cái, Hộp, Thùng, Kg, Lít (qua <c>to_uom_id</c>).
    /// </summary>
    public Uom ToUom { get; set; } = null!;

    #endregion
}
