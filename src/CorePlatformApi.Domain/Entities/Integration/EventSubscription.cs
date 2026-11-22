// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Service nào tiêu thụ event nào (bản đồ luồng dữ liệu giữa các phân hệ)
/// </summary>
/// <remarks>Bảng <c>integration.event_subscriptions</c>.</remarks>
public class EventSubscription : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Event
    /// </summary>
    /// <remarks>FK → <c>integration.event_types(code)</c></remarks>
    public string EventTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Tên service tiêu thụ (wms-api, accounting-api)
    /// </summary>
    public string ConsumerService { get; set; } = string.Empty;

    /// <summary>
    /// Queue nhận
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Handler xử lý (tham khảo)
    /// </summary>
    public string? HandlerName { get; set; }

    /// <summary>
    /// Số lần retry trước khi vào DLQ
    /// </summary>
    /// <remarks>Mặc định DB: <c>5</c></remarks>
    public short MaxRetries { get; set; } = 5;

    /// <summary>
    /// Đang hoạt động
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục event nghiệp vụ trao đổi giữa các phân hệ (crm (qua <c>event_type_code</c>).
    /// </summary>
    public IntegrationEventType EventType { get; set; } = null!;

    #endregion
}
