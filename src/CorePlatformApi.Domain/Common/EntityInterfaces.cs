namespace CorePlatformApi.Domain.Common;

/// <summary>
/// Entity có 4 cột audit chuẩn.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Entity hỗ trợ xóa mềm.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}

/// <summary>
/// Entity luôn thuộc về một tenant (tenant_id NOT NULL).
/// </summary>
public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}

/// <summary>
/// Entity có thể thuộc một tenant hoặc dùng chung toàn hệ thống (tenant_id NULL).
/// </summary>
public interface IMayHaveTenant
{
    Guid? TenantId { get; set; }
}
