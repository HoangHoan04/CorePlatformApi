// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Danh mục event nghiệp vụ trao đổi giữa các phân hệ (crm.order.confirmed, hrm.payroll.closed...) kèm schema payload
/// </summary>
/// <remarks>Bảng <c>integration.event_types</c>.</remarks>
public class IntegrationEventType : AuditableEntity
{
    /// <summary>
    /// Mã event (domain.entity.action)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ phát (producer)
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Tên event
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Phiên bản schema payload
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// JSON Schema của payload (validate + sinh tài liệu)
    /// </summary>
    public string? PayloadSchema { get; set; }

    /// <summary>
    /// Exchange/stream trên RabbitMQ/Redis Streams
    /// </summary>
    public string? ExchangeName { get; set; }

    /// <summary>
    /// Routing key
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// Cho phép đối tác đăng ký qua webhook
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module Module { get; set; } = null!;

    #endregion
}
