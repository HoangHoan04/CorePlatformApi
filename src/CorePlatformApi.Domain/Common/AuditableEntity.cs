namespace CorePlatformApi.Domain.Common;

/// <summary>
/// Gốc cho các bảng nghiệp vụ có 4 cột audit chuẩn (created_at, created_by, updated_at, updated_by).
/// </summary>
/// <remarks>
/// <see cref="RowVersion"/> ánh xạ cột hệ thống <c>xmin</c> của PostgreSQL để làm optimistic concurrency
/// (không tạo cột vật lý).
/// </remarks>
public abstract class AuditableEntity : IAuditableEntity
{
    /// <summary>
    /// Thời điểm tạo bản ghi
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Người tạo (iam.users.id); NULL nếu hệ thống tạo
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất (trigger tự gán)
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Người cập nhật gần nhất (iam.users.id)
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Phiên bản dòng (xmin) dùng cho optimistic concurrency.
    /// </summary>
    public uint RowVersion { get; set; }
}
