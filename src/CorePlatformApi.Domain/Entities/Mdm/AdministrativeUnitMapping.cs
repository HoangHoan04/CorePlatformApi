// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Bảng ánh xạ đơn vị hành chính cũ → mới (sau sáp nhập), dùng để chuyển đổi địa chỉ lịch sử
/// </summary>
/// <remarks>Bảng <c>mdm.administrative_unit_mappings</c>.</remarks>
public class AdministrativeUnitMapping : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Đơn vị cũ
    /// </summary>
    /// <remarks>FK → <c>mdm.administrative_units(id)</c></remarks>
    public Guid OldUnitId { get; set; }

    /// <summary>
    /// Đơn vị mới thay thế
    /// </summary>
    /// <remarks>FK → <c>mdm.administrative_units(id)</c></remarks>
    public Guid NewUnitId { get; set; }

    /// <summary>
    /// Hình thức: sáp nhập, chia tách, đổi tên, một phần
    /// </summary>
    /// <remarks>Ràng buộc: <c>mapping_type IN ('MERGE','SPLIT','RENAME','PARTIAL')</c>; Mặc định DB: <c>'MERGE'</c></remarks>
    public string MappingType { get; set; } = "MERGE";

    /// <summary>
    /// Ngày có hiệu lực theo nghị quyết
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// Số nghị quyết/văn bản pháp lý
    /// </summary>
    public string? LegalReference { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Note { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã) (qua <c>old_unit_id</c>).
    /// </summary>
    public AdministrativeUnit OldUnit { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã) (qua <c>new_unit_id</c>).
    /// </summary>
    public AdministrativeUnit NewUnit { get; set; } = null!;

    #endregion
}
