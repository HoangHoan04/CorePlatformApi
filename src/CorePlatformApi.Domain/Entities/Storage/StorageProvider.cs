// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Cấu hình nhà cung cấp Object Storage (MinIO, S3, R2...). Hỗ trợ tenant dùng storage riêng
/// </summary>
/// <remarks>Bảng <c>storage_providers</c> (phân hệ storage).</remarks>
public class StorageProvider : BaseEntity
{
    /// <summary>
    /// Mã cấu hình
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Loại provider
    /// </summary>
    /// <remarks>Ràng buộc: <c>provider_type IN ('MINIO','S3','R2','GCS','AZURE_BLOB','LOCAL')</c></remarks>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint (https://minio.internal:9000)
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// URL công khai/CDN cho file public
    /// </summary>
    public string? PublicBaseUrl { get; set; }

    /// <summary>
    /// Region
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Access key đã mã hóa
    /// </summary>
    public string? AccessKeyEncrypted { get; set; }

    /// <summary>
    /// Secret key đã mã hóa
    /// </summary>
    public string? SecretKeyEncrypted { get; set; }

    /// <summary>
    /// Path-style URL (MinIO cần true)
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool UsePathStyle { get; set; } = true;

    /// <summary>
    /// Provider mặc định
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Đang dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}
