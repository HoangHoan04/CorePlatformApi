// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Biến thể của file ảnh sau xử lý (thumbnail, small, medium, webp)
/// </summary>
/// <remarks>Bảng <c>storage.file_variants</c>.</remarks>
public class FileVariant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// File gốc
    /// </summary>
    /// <remarks>FK → <c>storage.files(id)</c></remarks>
    public Guid FileId { get; set; }

    /// <summary>
    /// Tên biến thể (THUMB, SMALL, MEDIUM, LARGE)
    /// </summary>
    public string VariantName { get; set; } = string.Empty;

    /// <summary>
    /// Object key biến thể
    /// </summary>
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// MIME type
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Dung lượng
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Rộng (px)
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Cao (px)
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Thời điểm tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>file_id</c>).
    /// </summary>
    public StorageFile File { get; set; } = null!;

    #endregion
}
