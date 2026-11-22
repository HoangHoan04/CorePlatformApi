// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Khóa ký JWT (RS256/ES256) phục vụ JWKS và xoay vòng khóa định kỳ
/// </summary>
/// <remarks>Bảng <c>iam.signing_keys</c>.</remarks>
public class SigningKey
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Key ID (header kid trong JWT)
    /// </summary>
    public string Kid { get; set; } = string.Empty;

    /// <summary>
    /// Thuật toán ký
    /// </summary>
    /// <remarks>Ràng buộc: <c>algorithm IN ('RS256','RS384','RS512','ES256','ES384','PS256')</c>; Mặc định DB: <c>'RS256'</c></remarks>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Mục đích
    /// </summary>
    /// <remarks>Ràng buộc: <c>usage IN ('SIGNING','ENCRYPTION')</c>; Mặc định DB: <c>'SIGNING'</c></remarks>
    public string Usage { get; set; } = "SIGNING";

    /// <summary>
    /// Public key (công bố qua /.well-known/jwks.json)
    /// </summary>
    public string PublicKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// Private key đã mã hóa
    /// </summary>
    public string PrivateKeyEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// PENDING: công bố trước, ACTIVE: đang ký, RETIRED: chỉ còn verify
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','ACTIVE','RETIRED','REVOKED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Bắt đầu ký
    /// </summary>
    public DateTimeOffset? ActivatedAt { get; set; }

    /// <summary>
    /// Ngừng ký
    /// </summary>
    public DateTimeOffset? RetiredAt { get; set; }

    /// <summary>
    /// Gỡ khỏi JWKS sau thời điểm này
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Thời điểm tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
