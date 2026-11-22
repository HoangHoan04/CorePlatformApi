// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Kết nối tới hệ thống bên thứ 3 (lưu credential/OAuth token): sàn TMĐT, nhà cung cấp HĐĐT, ngân hàng, Zalo OA...
/// </summary>
/// <remarks>Bảng <c>integration.connections</c>.</remarks>
public class Connection : SoftDeletableEntity, IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Công ty sở hữu kết nối
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Nhà cung cấp (VNPT_EINVOICE, VIETTEL_SINVOICE, MISA_MEINVOICE, SHOPEE, ZALO_OA, GHN, GHTK...)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Tên kết nối
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kiểu xác thực
    /// </summary>
    /// <remarks>Ràng buộc: <c>auth_type IN ('API_KEY','BASIC','OAUTH2','CERTIFICATE','CUSTOM')</c></remarks>
    public string AuthType { get; set; } = string.Empty;

    /// <summary>
    /// URL API
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Cấu hình không bí mật (mẫu số HĐ, ký hiệu, shop_id...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Config { get; set; } = "{}";

    /// <summary>
    /// Credential đã mã hóa (username/password/api key/chứng thư)
    /// </summary>
    public string? CredentialsEncrypted { get; set; }

    /// <summary>
    /// OAuth access token đã mã hóa
    /// </summary>
    public string? AccessTokenEncrypted { get; set; }

    /// <summary>
    /// OAuth refresh token đã mã hóa
    /// </summary>
    public string? RefreshTokenEncrypted { get; set; }

    /// <summary>
    /// Hạn access token (job tự refresh)
    /// </summary>
    public DateTimeOffset? TokenExpiresAt { get; set; }

    /// <summary>
    /// Id tài khoản phía đối tác (shop_id, oa_id)
    /// </summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>
    /// Đồng bộ gần nhất
    /// </summary>
    public DateTimeOffset? LastSyncAt { get; set; }

    /// <summary>
    /// Lỗi gần nhất
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','ACTIVE','EXPIRED','ERROR','DISABLED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Danh sách ExternalMapping con (qua <c>integration.external_mappings.connection_id</c>).
    /// </summary>
    public ICollection<ExternalMapping> ExternalMappings { get; set; } = [];

    #endregion
}
