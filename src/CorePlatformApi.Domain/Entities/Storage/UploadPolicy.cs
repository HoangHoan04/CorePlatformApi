// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Chính sách upload theo loại file: dung lượng, định dạng, nén/resize ảnh, bucket đích, mức nhạy cảm
/// </summary>
/// <remarks>Bảng <c>storage.upload_policies</c>.</remarks>
public class UploadPolicy : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = mặc định)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Loại file (AVATAR, LABOR_CONTRACT, PAYSLIP, INVOICE, PRODUCT_IMAGE, ATTACHMENT, KB_DOCUMENT)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ sử dụng
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Bucket lưu
    /// </summary>
    /// <remarks>FK → <c>storage.buckets(id)</c></remarks>
    public Guid BucketId { get; set; }

    /// <summary>
    /// Mẫu object key
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{tenant}/{module}/{yyyy}/{mm}/{uuid}{ext}'</c></remarks>
    public string PathTemplate { get; set; } = "{tenant}/{module}/{yyyy}/{mm}/{uuid}{ext}";

    /// <summary>
    /// Dung lượng tối đa/file (MB)
    /// </summary>
    /// <remarks>Mặc định DB: <c>10</c></remarks>
    public decimal MaxSizeMb { get; set; } = 10m;

    /// <summary>
    /// MIME cho phép (NULL = tất cả trừ blacklist)
    /// </summary>
    public List<string>? AllowedMimeTypes { get; set; }

    /// <summary>
    /// Đuôi file cho phép
    /// </summary>
    public List<string>? AllowedExtensions { get; set; }

    /// <summary>
    /// Mức nhạy cảm: RESTRICTED (bảng lương, HĐLĐ) bắt buộc presigned URL ngắn hạn + ghi log truy cập
    /// </summary>
    /// <remarks>Ràng buộc: <c>sensitivity IN ('PUBLIC','INTERNAL','CONFIDENTIAL','RESTRICTED')</c>; Mặc định DB: <c>'INTERNAL'</c></remarks>
    public string Sensitivity { get; set; } = "INTERNAL";

    /// <summary>
    /// Thời hạn link tạm (giây)
    /// </summary>
    /// <remarks>Mặc định DB: <c>300</c></remarks>
    public int PresignedTtlSeconds { get; set; } = 300;

    /// <summary>
    /// Tự resize ảnh
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool AutoResize { get; set; } = false;

    /// <summary>
    /// Chiều rộng tối đa sau resize (px)
    /// </summary>
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Chiều cao tối đa (px)
    /// </summary>
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Chất lượng nén ảnh
    /// </summary>
    /// <remarks>Ràng buộc: <c>image_quality BETWEEN 1 AND 100</c>; Mặc định DB: <c>80</c></remarks>
    public short? ImageQuality { get; set; } = 80;

    /// <summary>
    /// Chuyển định dạng ảnh
    /// </summary>
    /// <remarks>Ràng buộc: <c>convert_to_format IN ('WEBP','JPEG','PNG','AVIF')</c></remarks>
    public string? ConvertToFormat { get; set; }

    /// <summary>
    /// Biến thể cần sinh, VD: [{"name":"THUMB","w":150,"h":150,"fit":"cover"}]
    /// </summary>
    public string? Variants { get; set; }

    /// <summary>
    /// Xóa EXIF (GPS) khi upload từ mobile
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool StripExif { get; set; } = true;

    /// <summary>
    /// Quét virus
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool VirusScan { get; set; } = true;

    /// <summary>
    /// File tạm (chưa gắn entity) bị dọn sau N giờ
    /// </summary>
    /// <remarks>Mặc định DB: <c>24</c></remarks>
    public int TempTtlHours { get; set; } = 24;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Điều hướng tới Bucket lưu trữ theo mục đích (public-assets, private-docs, hr-sensitive (qua <c>bucket_id</c>).
    /// </summary>
    public StorageBucket Bucket { get; set; } = null!;

    #endregion
}
