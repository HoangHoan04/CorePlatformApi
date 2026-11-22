// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Monitor;

/// <summary>
/// Kết quả từng lần health check
/// </summary>
/// <remarks>Bảng <c>monitor.health_checks</c>; partition RANGE theo <c>checked_at</c> (tạo bằng SQL thô, không qua EF migration).</remarks>
public class HealthCheck
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Service
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Kết quả
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('HEALTHY','DEGRADED','UNHEALTHY')</c></remarks>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status
    /// </summary>
    public short? HttpStatus { get; set; }

    /// <summary>
    /// Thời gian phản hồi
    /// </summary>
    public int? ResponseMs { get; set; }

    /// <summary>
    /// Chi tiết từng dependency (db, redis, rabbitmq) trả về từ /health
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Lỗi
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Thời điểm
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
}
