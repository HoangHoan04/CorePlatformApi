// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Quy tắc sinh mã tự động theo loại chứng từ, có thể riêng theo công ty/chi nhánh. Pattern hỗ trợ token {YYYY} {YY} {MM} {DD} {YYMM} {BRANCH} {COMPANY} {0001}
/// </summary>
/// <remarks>Bảng <c>mdm.numbering_rules</c>.</remarks>
public class NumberingRule : SoftDeletableEntity, IMustHaveTenant
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
    /// Loại chứng từ
    /// </summary>
    /// <remarks>FK → <c>mdm.document_types(code)</c></remarks>
    public string DocumentTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Áp dụng riêng công ty (NULL = toàn tenant)
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Áp dụng riêng chi nhánh (ưu tiên cao nhất)
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Tên quy tắc
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mẫu đầy đủ, VD: 'PNK-{BRANCH}-{00001}', 'NV-{YYYY}-{0001}'. Nếu NULL thì ghép prefix + số + suffix
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Tiền tố (có thể chứa token)
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Hậu tố (có thể chứa token)
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// Độ dài phần số tự tăng (khi không dùng pattern)
    /// </summary>
    /// <remarks>Ràng buộc: <c>padding_length BETWEEN 1 AND 12</c>; Mặc định DB: <c>4</c></remarks>
    public short PaddingLength { get; set; } = 4;

    /// <summary>
    /// Giá trị bắt đầu
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public long StartValue { get; set; } = 1;

    /// <summary>
    /// Bước tăng
    /// </summary>
    /// <remarks>Ràng buộc: <c>step &gt; 0</c>; Mặc định DB: <c>1</c></remarks>
    public int Step { get; set; } = 1;

    /// <summary>
    /// Chu kỳ reset số đếm
    /// </summary>
    /// <remarks>Ràng buộc: <c>reset_policy IN ('NEVER','YEARLY','MONTHLY','DAILY')</c>; Mặc định DB: <c>'NEVER'</c></remarks>
    public string ResetPolicy { get; set; } = "NEVER";

    /// <summary>
    /// Số đếm dùng chung theo quy tắc hay tách riêng từng công ty/chi nhánh
    /// </summary>
    /// <remarks>Ràng buộc: <c>counter_scope IN ('RULE','COMPANY','BRANCH')</c>; Mặc định DB: <c>'RULE'</c></remarks>
    public string CounterScope { get; set; } = "RULE";

    /// <summary>
    /// Cho phép người dùng tự nhập mã
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool AllowManual { get; set; } = false;

    /// <summary>
    /// Ví dụ mã sinh ra (hiển thị UI)
    /// </summary>
    public string? Preview { get; set; }

    /// <summary>
    /// Đang áp dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo (qua <c>document_type_code</c>).
    /// </summary>
    public DocumentType DocumentType { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Danh sách NumberingCounter con (qua <c>mdm.numbering_counters.rule_id</c>).
    /// </summary>
    public ICollection<NumberingCounter> NumberingCounters { get; set; } = [];

    #endregion
}
