using System.ComponentModel.DataAnnotations;

namespace CorePlatformApi.Domain.Common;

/// <summary>
/// Entity gốc của mọi bảng: khóa chính UUID v7 (tăng dần theo thời gian, thân thiện với index),
/// tenant, audit, xóa mềm và phiên bản dòng.
/// </summary>
/// <remarks>
/// <see cref="TenantId"/> NULL = dữ liệu dùng chung toàn hệ thống (danh mục, quyền, Platform Admin...);
/// bảng nghiệp vụ của tenant cấu hình cột này NOT NULL.
/// <see cref="Version"/> ánh xạ cột hệ thống <c>xmin</c> của PostgreSQL (optimistic concurrency, không tạo cột vật lý).
/// Mọi truy vấn tự ẩn bản ghi <see cref="IsDeleted"/> = true; dùng <c>IgnoreQueryFilters()</c> để xem cả bản ghi đã xóa.
/// </remarks>
public abstract class BaseEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid? TenantId { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    [Required]
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}

/// <summary>
/// Đánh dấu bảng nghiệp vụ dùng xóa mềm: <c>Remove()</c> chỉ gán IsDeleted = true thay vì xóa dòng.
/// Bảng không gắn attribute này bị xóa thật khi <c>Remove()</c> (tránh vướng ràng buộc UNIQUE khi thêm lại).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class SoftDeleteAttribute : Attribute;
