// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Nhật ký truy cập file nhạy cảm (xem, tải, cấp presigned URL) phục vụ kiểm toán
/// </summary>
/// <remarks>Bảng <c>file_access_logs</c> (phân hệ storage).</remarks>
public class FileAccessLog : BaseEntity
{
    /// <summary>
    /// File
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// Người truy cập (NULL nếu qua share link)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Link chia sẻ được dùng
    /// </summary>
    public Guid? ShareLinkId { get; set; }

    /// <summary>
    /// Hành động
    /// </summary>
    /// <remarks>Ràng buộc: <c>action IN ('VIEW','DOWNLOAD','PRESIGN','DELETE','SHARE')</c></remarks>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// IP
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Thời điểm
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
