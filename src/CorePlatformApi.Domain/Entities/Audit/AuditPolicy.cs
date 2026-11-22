// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Audit;

/// <summary>
/// Cấu hình đối tượng cần lưu vết thay đổi và trường cần che (lương, CCCD...)
/// </summary>
/// <remarks>Bảng <c>audit.audit_policies</c>.</remarks>
public class AuditPolicy : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Phân hệ
    /// </summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Loại đối tượng (EMPLOYEE, SALARY, STOCK_ADJUSTMENT)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Bật lưu vết
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Hành động cần ghi
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{INSERT,UPDATE,DELETE}'</c></remarks>
    public List<string> TrackActions { get; set; } = ["INSERT", "UPDATE", "DELETE"];

    /// <summary>
    /// Chỉ ghi các trường này (NULL = tất cả)
    /// </summary>
    public List<string>? IncludeFields { get; set; }

    /// <summary>
    /// Bỏ qua trường này
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{updated_at,updated_by}'</c></remarks>
    public List<string> ExcludeFields { get; set; } = ["updated_at", "updated_by"];

    /// <summary>
    /// Trường được ghi nhưng che giá trị (***), VD: base_salary, id_number
    /// </summary>
    public List<string>? MaskedFields { get; set; }

    /// <summary>
    /// Thời gian lưu (ngày) — chứng từ kế toán tối thiểu 10 năm
    /// </summary>
    /// <remarks>Mặc định DB: <c>3650</c></remarks>
    public int RetentionDays { get; set; } = 3650;
}
