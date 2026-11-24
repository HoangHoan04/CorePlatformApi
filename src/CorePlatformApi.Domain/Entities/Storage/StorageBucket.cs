// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Bucket lưu trữ theo mục đích (public-assets, private-docs, hr-sensitive...)
/// </summary>
/// <remarks>Bảng <c>buckets</c> (phân hệ storage).</remarks>
public class StorageBucket : BaseEntity
{
    /// <summary>
    /// Provider
    /// </summary>
    /// <remarks>FK → <c>storage.providers(id)</c></remarks>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Tên bucket thật trên storage
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mục đích sử dụng
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Cho phép đọc công khai
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Bật versioning trên storage
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool Versioning { get; set; } = false;

    /// <summary>
    /// Mã hóa phía server
    /// </summary>
    /// <remarks>Ràng buộc: <c>encryption IN ('NONE','SSE','SSE_KMS')</c>; Mặc định DB: <c>'SSE'</c></remarks>
    public string Encryption { get; set; } = "SSE";

    /// <summary>
    /// Tự xóa file sau N ngày (lifecycle)
    /// </summary>
    public int? RetentionDays { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Cấu hình nhà cung cấp Object Storage (MinIO, S3, R2 (qua <c>provider_id</c>).
    /// </summary>
    public StorageProvider Provider { get; set; } = null!;

    #endregion
}
