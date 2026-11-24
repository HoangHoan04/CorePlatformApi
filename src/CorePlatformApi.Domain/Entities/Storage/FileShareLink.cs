// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Storage;

/// <summary>
/// Link chia sẻ file có kiểm soát (hết hạn, giới hạn lượt tải, mật khẩu) — khác presigned URL ngắn hạn của S3
/// </summary>
/// <remarks>Bảng <c>file_share_links</c> (phân hệ storage).</remarks>
public class FileShareLink : BaseEntity
{
    /// <summary>
    /// File
    /// </summary>
    /// <remarks>FK → <c>storage.files(id)</c></remarks>
    public Guid FileId { get; set; }

    /// <summary>
    /// Hash token trong URL chia sẻ
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu mở link (tùy chọn)
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Hết hạn
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Số lượt tải tối đa
    /// </summary>
    public int? MaxDownloads { get; set; }

    /// <summary>
    /// Đã tải
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int DownloadCount { get; set; } = 0;

    /// <summary>
    /// Chỉ cho phép email này (tùy chọn)
    /// </summary>
    public List<string>? AllowedEmails { get; set; }

    /// <summary>
    /// Thu hồi lúc
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>file_id</c>).
    /// </summary>
    public StorageFile File { get; set; } = null!;

    #endregion
}
