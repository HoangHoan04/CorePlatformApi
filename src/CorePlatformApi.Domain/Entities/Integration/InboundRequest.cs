// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Nhật ký request inbound nhận được (lưu raw để đối soát, xử lý lại, chống trùng)
/// </summary>
/// <remarks>Bảng <c>integration.inbound_requests</c>.</remarks>
public class InboundRequest : IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Nguồn
    /// </summary>
    /// <remarks>FK → <c>integration.inbound_sources(id)</c></remarks>
    public Guid SourceId { get; set; }

    /// <summary>
    /// Tenant
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Id giao dịch phía ngoài (mã GD ngân hàng, order_sn) để chống trùng
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Phương thức
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Header nhận được
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    /// Query string (VNPay IPN dùng query)
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// Body gốc (giữ nguyên để verify chữ ký)
    /// </summary>
    public string? RawBody { get; set; }

    /// <summary>
    /// Payload đã parse/mapping
    /// </summary>
    public string? ParsedPayload { get; set; }

    /// <summary>
    /// IP gửi
    /// </summary>
    public IPAddress? SourceIp { get; set; }

    /// <summary>
    /// Kết quả kiểm tra chữ ký
    /// </summary>
    public bool? SignatureValid { get; set; }

    /// <summary>
    /// Trạng thái xử lý
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('RECEIVED','PROCESSING','PROCESSED','FAILED','DUPLICATE','REJECTED')</c>; Mặc định DB: <c>'RECEIVED'</c></remarks>
    public string Status { get; set; } = "RECEIVED";

    /// <summary>
    /// HTTP status đã trả cho bên gửi
    /// </summary>
    public short? ResponseStatus { get; set; }

    /// <summary>
    /// Body đã trả (VNPay yêu cầu RspCode)
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Event nội bộ đã phát (outbox_messages.id)
    /// </summary>
    public Guid? EmittedMessageId { get; set; }

    /// <summary>
    /// Lỗi xử lý
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Số lần xử lý lại
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short RetryCount { get; set; } = 0;

    /// <summary>
    /// Nhận lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Xử lý xong lúc
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Nguồn webhook INBOUND: cổng thanh toán (VNPay, SePay, VietQR), sàn TMĐT, máy chấm công, hóa đơn điện tử (qua <c>source_id</c>).
    /// </summary>
    public InboundSource Source { get; set; } = null!;

    #endregion
}
