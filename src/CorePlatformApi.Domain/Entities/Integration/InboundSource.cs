// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Nguồn webhook INBOUND: cổng thanh toán (VNPay, SePay, VietQR), sàn TMĐT, máy chấm công, hóa đơn điện tử
/// </summary>
/// <remarks>Bảng <c>inbound_sources</c> (phân hệ integration); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class InboundSource : BaseEntity
{
    /// <summary>
    /// Mã nguồn (SEPAY_MAIN, SHOPEE_STORE1)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    /// <remarks>Ràng buộc: <c>provider IN ('VNPAY','SEPAY','VIETQR','MOMO','PAYOS','SHOPEE','TIKTOK_SHOP','LAZADA','TIKI','ZALO_OA','ATTENDANCE_DEVICE','EINVOICE','BANK','CUSTOM')</c></remarks>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Token ngẫu nhiên trong URL nhận: /integration/inbound/{endpoint_token}
    /// </summary>
    public string EndpointToken { get; set; } = string.Empty;

    /// <summary>
    /// Cách xác thực request đến
    /// </summary>
    /// <remarks>Ràng buộc: <c>auth_type IN ('NONE','HMAC','API_KEY','BASIC','CHECKSUM','JWT','IP_ONLY')</c>; Mặc định DB: <c>'HMAC'</c></remarks>
    public string AuthType { get; set; } = "HMAC";

    /// <summary>
    /// Secret/checksum key đã mã hóa
    /// </summary>
    public string? SecretEncrypted { get; set; }

    /// <summary>
    /// Header chứa chữ ký
    /// </summary>
    public string? SignatureHeader { get; set; }

    /// <summary>
    /// Thuật toán (HMAC_SHA256, HMAC_SHA512, MD5)
    /// </summary>
    public string? SignatureAlgorithm { get; set; }

    /// <summary>
    /// IP nguồn được phép
    /// </summary>
    public List<IPNetwork>? IpWhitelist { get; set; }

    /// <summary>
    /// Event nội bộ phát ra sau khi xác thực (integration.event_types)
    /// </summary>
    public string? TargetEventType { get; set; }

    /// <summary>
    /// Mapping payload ngoài → payload nội bộ (JSONata/Liquid)
    /// </summary>
    public string? MappingScript { get; set; }

    /// <summary>
    /// Công ty nhận dữ liệu
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Chi nhánh nhận dữ liệu (máy chấm công tại chi nhánh)
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Cấu hình riêng (số tài khoản nhận tiền, merchant id...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Config { get; set; } = "{}";

    /// <summary>
    /// Lần nhận gần nhất
    /// </summary>
    public DateTimeOffset? LastReceivedAt { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','PAUSED','DISABLED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
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
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    #endregion
}
