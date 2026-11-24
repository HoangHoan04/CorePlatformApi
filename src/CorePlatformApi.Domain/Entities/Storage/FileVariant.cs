// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Biến thể của file ảnh sau xử lý (thumbnail, small, medium, webp)
/// </summary>
/// <remarks>Bảng <c>file_variants</c> (phân hệ storage).</remarks>
public class FileVariant : BaseEntity
{
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

    #region Navigations

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>file_id</c>).
    /// </summary>
    public StorageFile File { get; set; } = null!;

    #endregion
}
