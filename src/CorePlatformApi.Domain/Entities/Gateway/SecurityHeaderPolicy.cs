// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Bộ security header gắn vào response (HSTS, CSP, X-Frame-Options...)
/// </summary>
/// <remarks>Bảng <c>security_header_policies</c> (phân hệ gateway).</remarks>
public class SecurityHeaderPolicy : BaseEntity
{
    /// <summary>
    /// Mã chính sách (default, strict-admin)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Map header → giá trị, VD: {"Strict-Transport-Security":"max-age=31536000; includeSubDomains"}
    /// </summary>
    public string Headers { get; set; } = string.Empty;

    /// <summary>
    /// Header cần gỡ khỏi response
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{Server,X-Powered-By}'</c></remarks>
    public List<string> RemoveHeaders { get; set; } = ["Server", "X-Powered-By"];

    /// <summary>
    /// Chuyển hướng HTTP → HTTPS
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool ForceHttps { get; set; } = true;
}
