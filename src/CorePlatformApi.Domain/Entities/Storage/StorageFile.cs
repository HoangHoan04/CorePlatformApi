// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Metadata file đã upload. File vật lý nằm trên Object Storage; mọi phân hệ chỉ lưu file_id
/// </summary>
/// <remarks>Bảng <c>storage.files</c>.</remarks>
public class StorageFile : SoftDeletableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính (file_id dùng ở các phân hệ)
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Bucket
    /// </summary>
    /// <remarks>FK → <c>storage.buckets(id)</c></remarks>
    public Guid BucketId { get; set; }

    /// <summary>
    /// Đường dẫn object trên storage
    /// </summary>
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Tên file gốc người dùng upload
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// Đuôi file (.pdf)
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// MIME type đã kiểm tra bằng magic bytes
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Dung lượng (byte)
    /// </summary>
    /// <remarks>Ràng buộc: <c>size_bytes &gt;= 0</c></remarks>
    public long SizeBytes { get; set; }

    /// <summary>
    /// SHA-256 phát hiện trùng/toàn vẹn
    /// </summary>
    public string? ChecksumSha256 { get; set; }

    /// <summary>
    /// Loại file (theo storage.upload_policies.category)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'ATTACHMENT'</c></remarks>
    public string Category { get; set; } = "ATTACHMENT";

    /// <summary>
    /// Phân hệ sở hữu (HRM, WMS...)
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Loại đối tượng gắn (EMPLOYEE, LEAVE_REQUEST, GOODS_RECEIPT)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Id đối tượng gắn
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Trường của đối tượng (avatar, contract_scan, attachments)
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Mức nhạy cảm
    /// </summary>
    /// <remarks>Ràng buộc: <c>sensitivity IN ('PUBLIC','INTERNAL','CONFIDENTIAL','RESTRICTED')</c>; Mặc định DB: <c>'INTERNAL'</c></remarks>
    public string Sensitivity { get; set; } = "INTERNAL";

    /// <summary>
    /// TEMP: đã upload chưa gắn entity; ACTIVE: đã gắn; QUARANTINED: nghi nhiễm virus
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('UPLOADING','TEMP','ACTIVE','QUARANTINED','DELETED')</c>; Mặc định DB: <c>'TEMP'</c></remarks>
    public string Status { get; set; } = "TEMP";

    /// <summary>
    /// Kết quả quét virus
    /// </summary>
    /// <remarks>Ràng buộc: <c>scan_status IN ('PENDING','CLEAN','INFECTED','SKIPPED','ERROR')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string ScanStatus { get; set; } = "PENDING";

    /// <summary>
    /// Chiều rộng ảnh (px)
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Chiều cao ảnh (px)
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Thời lượng (video/audio)
    /// </summary>
    public decimal? DurationSeconds { get; set; }

    /// <summary>
    /// Số trang (PDF)
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Phiên bản của tài liệu
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public int VersionNo { get; set; } = 1;

    /// <summary>
    /// Phiên bản trước
    /// </summary>
    /// <remarks>FK → <c>storage.files(id)</c></remarks>
    public Guid? PreviousVersionId { get; set; }

    /// <summary>
    /// Nguồn upload
    /// </summary>
    /// <remarks>Ràng buộc: <c>upload_source IN ('WEB','MOBILE','API','SYSTEM','IMPORT')</c>; Mặc định DB: <c>'WEB'</c></remarks>
    public string UploadSource { get; set; } = "WEB";

    /// <summary>
    /// User upload
    /// </summary>
    public Guid? UploadedBy { get; set; }

    /// <summary>
    /// IP upload
    /// </summary>
    public IPAddress? UploadedIp { get; set; }

    /// <summary>
    /// File TEMP hết hạn sẽ bị job dọn
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Metadata mở rộng (EXIF đã lọc, tag...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Metadata { get; set; } = "{}";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Bucket lưu trữ theo mục đích (public-assets, private-docs, hr-sensitive (qua <c>bucket_id</c>).
    /// </summary>
    public StorageBucket Bucket { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>previous_version_id</c>).
    /// </summary>
    public StorageFile? PreviousVersion { get; set; }

    /// <summary>
    /// Danh sách FileVariant con (qua <c>storage.file_variants.file_id</c>).
    /// </summary>
    public ICollection<FileVariant> FileVariants { get; set; } = [];

    /// <summary>
    /// Danh sách FileShareLink con (qua <c>storage.file_share_links.file_id</c>).
    /// </summary>
    public ICollection<FileShareLink> FileShareLinks { get; set; } = [];

    #endregion
}
