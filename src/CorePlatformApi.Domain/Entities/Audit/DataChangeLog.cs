// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Audit;

/// <summary>
/// Lịch sử thay đổi dữ liệu nhạy cảm (old/new value) phục vụ đối soát kế toán, kho, nhân sự. Các phân hệ gửi qua event hoặc ghi trực tiếp
/// </summary>
/// <remarks>Bảng <c>data_change_logs</c> (phân hệ audit).</remarks>
public class DataChangeLog : BaseEntity
{
    /// <summary>
    /// Công ty (lọc theo data scope)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Phân hệ
    /// </summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Loại đối tượng
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Id đối tượng (text để hỗ trợ khóa không phải uuid)
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Tên/mã hiển thị (NV-2026-0001 - Nguyễn Văn A)
    /// </summary>
    public string? EntityDisplay { get; set; }

    /// <summary>
    /// Hành động
    /// </summary>
    /// <remarks>Ràng buộc: <c>action IN ('INSERT','UPDATE','DELETE','RESTORE','APPROVE','EXPORT')</c></remarks>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách trường thay đổi
    /// </summary>
    public List<string>? ChangedFields { get; set; }

    /// <summary>
    /// Giá trị cũ {field: value}
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Giá trị mới {field: value}
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Lý do thay đổi (bắt buộc với một số nghiệp vụ)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Người thay đổi
    /// </summary>
    public Guid? ChangedBy { get; set; }

    /// <summary>
    /// Tên người thay đổi (snapshot)
    /// </summary>
    public string? ChangedByName { get; set; }

    /// <summary>
    /// Thay mặt (impersonation)
    /// </summary>
    public Guid? OnBehalfOf { get; set; }

    /// <summary>
    /// Nguồn thay đổi
    /// </summary>
    /// <remarks>Ràng buộc: <c>source IN ('WEB','MOBILE','API','IMPORT','JOB','WORKFLOW','SYSTEM')</c>; Mặc định DB: <c>'WEB'</c></remarks>
    public string Source { get; set; } = "WEB";

    /// <summary>
    /// IP
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// Trace id liên kết request
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Thời điểm
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}
