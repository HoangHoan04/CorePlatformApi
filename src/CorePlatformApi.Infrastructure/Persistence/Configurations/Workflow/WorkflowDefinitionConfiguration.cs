// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.definitions</c>.
/// </summary>
public sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("definitions", "workflow", tb =>
        {
            tb.HasComment("Định nghĩa quy trình duyệt cho một loại chứng từ (đơn nghỉ phép, đề xuất mua hàng, chiết khấu vượt khung...)");
        });

        builder.HasKey(x => x.Id).HasName("definitions_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.DocumentTypeCode)
            .HasColumnName("document_type_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại chứng từ áp dụng");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã quy trình");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên quy trình");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Áp dụng riêng công ty (NULL = toàn tenant)");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Áp dụng riêng chi nhánh");
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Áp dụng riêng phòng ban");
        builder.Property(x => x.ApplicableCondition)
            .HasColumnName("applicable_condition")
            .HasColumnType("jsonb")
            .HasColumnOrder(9)
            .HasComment("Điều kiện chọn quy trình khi có nhiều quy trình cùng loại chứng từ");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Ưu tiên khi nhiều quy trình cùng khớp (số lớn ưu tiên)");
        builder.Property(x => x.CurrentVersionId)
            .HasColumnName("current_version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Phiên bản đang áp dụng (workflow.definition_versions)");
        builder.Property(x => x.AllowRecall)
            .HasColumnName("allow_recall")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Người tạo được thu hồi khi chưa có ai duyệt");
        builder.Property(x => x.AllowEditWhilePending)
            .HasColumnName("allow_edit_while_pending")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho sửa chứng từ khi đang chờ duyệt");
        builder.Property(x => x.AutoApproveIfRequesterIsApprover)
            .HasColumnName("auto_approve_if_requester_is_approver")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Tự bỏ qua bước nếu người duyệt chính là người tạo");
        builder.Property(x => x.SkipDuplicateApprover)
            .HasColumnName("skip_duplicate_approver")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Một người xuất hiện ở nhiều bước chỉ cần duyệt một lần");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang áp dụng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.DocumentTypeCode }, "ix_wf_definitions_doc")
            .HasFilter("is_active AND NOT is_deleted");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("definitions_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeCode)
            .HasConstraintName("definitions_document_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("definitions_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("definitions_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("definitions_department_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.CurrentVersion)
            .WithMany()
            .HasForeignKey(x => x.CurrentVersionId)
            .HasConstraintName("fk_wf_def_current_version")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
