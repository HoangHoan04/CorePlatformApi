namespace CorePlatformApi.Domain.Common;

/// <summary>
/// Gốc cho các bảng nghiệp vụ có 6 cột audit + cờ xóa mềm (is_deleted, deleted_at, deleted_by).
/// </summary>
public abstract class SoftDeletableEntity : AuditableEntity, ISoftDeletable
{
    /// <summary>
    /// Cờ xóa mềm (true = đã xóa)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Thời điểm xóa mềm
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Người xóa mềm (iam.users.id)
    /// </summary>
    public Guid? DeletedBy { get; set; }
}
