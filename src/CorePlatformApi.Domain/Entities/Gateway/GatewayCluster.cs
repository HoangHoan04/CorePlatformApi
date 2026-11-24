// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Cụm upstream (service nội bộ) mà gateway định tuyến tới
/// </summary>
/// <remarks>Bảng <c>clusters</c> (phân hệ gateway).</remarks>
public class GatewayCluster : BaseEntity
{
    /// <summary>
    /// Mã cluster (hrm-api, wms-api, auth-api)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Thuật toán cân bằng tải
    /// </summary>
    /// <remarks>Ràng buộc: <c>load_balancing IN ('ROUND_ROBIN','LEAST_REQUESTS','RANDOM','POWER_OF_TWO','FIRST')</c>; Mặc định DB: <c>'ROUND_ROBIN'</c></remarks>
    public string LoadBalancing { get; set; } = "ROUND_ROBIN";

    /// <summary>
    /// Đường dẫn health check chủ động
    /// </summary>
    /// <remarks>Mặc định DB: <c>'/health'</c></remarks>
    public string HealthCheckPath { get; set; } = "/health";

    /// <summary>
    /// Chu kỳ health check
    /// </summary>
    /// <remarks>Mặc định DB: <c>15</c></remarks>
    public int HealthCheckIntervalSeconds { get; set; } = 15;

    /// <summary>
    /// Timeout chuyển tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>30000</c></remarks>
    public int RequestTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Phiên bản HTTP tới upstream
    /// </summary>
    /// <remarks>Mặc định DB: <c>'1.1'</c></remarks>
    public string HttpVersion { get; set; } = "1.1";

    /// <summary>
    /// Đang dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Danh sách GatewayDestination con (qua <c>gateway.destinations.cluster_id</c>).
    /// </summary>
    public ICollection<GatewayDestination> GatewayDestinations { get; set; } = [];

    #endregion
}
