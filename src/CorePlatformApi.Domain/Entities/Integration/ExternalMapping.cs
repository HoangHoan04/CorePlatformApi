// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Ánh xạ id nội bộ ↔ id hệ thống ngoài (sản phẩm ERP ↔ item_id Shopee, NV ↔ mã trên máy chấm công)
/// </summary>
/// <remarks>Bảng <c>integration.external_mappings</c>.</remarks>
public class ExternalMapping : IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Kết nối
    /// </summary>
    /// <remarks>FK → <c>integration.connections(id)</c></remarks>
    public Guid? ConnectionId { get; set; }

    /// <summary>
    /// Loại đối tượng nội bộ (PRODUCT, EMPLOYEE, CUSTOMER)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Id nội bộ
    /// </summary>
    public Guid InternalId { get; set; }

    /// <summary>
    /// Hệ thống ngoài
    /// </summary>
    public string ExternalSystem { get; set; } = string.Empty;

    /// <summary>
    /// Id phía ngoài
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu bổ sung phía ngoài
    /// </summary>
    public string? ExternalData { get; set; }

    /// <summary>
    /// Đồng bộ gần nhất
    /// </summary>
    public DateTimeOffset? LastSyncedAt { get; set; }

    /// <summary>
    /// Tạo lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Kết nối tới hệ thống bên thứ 3 (lưu credential/OAuth token): sàn TMĐT, nhà cung cấp HĐĐT, ngân hàng, Zalo OA (qua <c>connection_id</c>).
    /// </summary>
    public Connection? Connection { get; set; }

    #endregion
}
