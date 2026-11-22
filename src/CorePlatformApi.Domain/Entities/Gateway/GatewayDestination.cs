// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Địa chỉ instance cụ thể của cluster (container trong Docker network)
/// </summary>
/// <remarks>Bảng <c>gateway.destinations</c>.</remarks>
public class GatewayDestination : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Cluster
    /// </summary>
    /// <remarks>FK → <c>gateway.clusters(id)</c></remarks>
    public Guid ClusterId { get; set; }

    /// <summary>
    /// Tên instance (hrm-api-1)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ nội bộ (http://hrm-api:8080)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Trọng số
    /// </summary>
    /// <remarks>Mặc định DB: <c>100</c></remarks>
    public short Weight { get; set; } = 100;

    /// <summary>
    /// Đang nhận traffic
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Cụm upstream (service nội bộ) mà gateway định tuyến tới (qua <c>cluster_id</c>).
    /// </summary>
    public GatewayCluster Cluster { get; set; } = null!;

    #endregion
}
