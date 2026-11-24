// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho bảng <c>instances</c>.
/// </summary>
public sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("instances", tb =>
        {
            tb.HasComment("Một lượt chạy quy trình cho một chứng từ cụ thể");
            tb.HasCheckConstraint("instances_status_check", "status IN ('DRAFT','PENDING','APPROVED','REJECTED','RETURNED','CANCELLED','RECALLED','ERROR')");
            tb.HasCheckConstraint("instances_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
        });

        builder.HasKey(x => x.Id).HasName("instances_pkey");

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
        builder.Property(x => x.DefinitionId)
            .HasColumnName("definition_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Quy trình");
        builder.Property(x => x.VersionId)
            .HasColumnName("version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Phiên bản đã dùng");
        builder.Property(x => x.DocumentTypeCode)
            .HasColumnName("document_type_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Loại chứng từ");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Phân hệ");
        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Id chứng từ bên phân hệ");
        builder.Property(x => x.DocumentCode)
            .HasColumnName("document_code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(7)
            .HasComment("Mã chứng từ hiển thị (DXMH-2611-0012)");
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasComment("Tiêu đề hiển thị trong danh sách chờ duyệt");
        builder.Property(x => x.Summary)
            .HasColumnName("summary")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("Mô tả ngắn");
        builder.Property(x => x.Variables)
            .HasColumnName("variables")
            .HasColumnType("jsonb")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Ảnh chụp biến dùng rẽ nhánh lúc gửi duyệt (amount, leave_days...)");
        builder.Property(x => x.DocumentUrl)
            .HasColumnName("document_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(11)
            .HasComment("Link mở chứng từ");
        builder.Property(x => x.RequesterId)
            .HasColumnName("requester_id")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .IsRequired()
            .HasComment("Người gửi duyệt");
        builder.Property(x => x.OnBehalfOfId)
            .HasColumnName("on_behalf_of_id")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Gửi thay cho ai (nếu có)");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Công ty của chứng từ");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Chi nhánh");
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Phòng ban");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái tổng");
        builder.Property(x => x.CurrentStepNames)
            .HasColumnName("current_step_names")
            .HasColumnType("text")
            .HasColumnOrder(18)
            .HasComment("Tên bước hiện tại (denormalize hiển thị)");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("'NORMAL'")
            .ValueGeneratedNever()
            .HasComment("Độ khẩn");
        builder.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Gửi duyệt lúc");
        builder.Property(x => x.DueAt)
            .HasColumnName("due_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Hạn hoàn thành toàn quy trình");
        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Kết thúc lúc");
        builder.Property(x => x.FinalComment)
            .HasColumnName("final_comment")
            .HasColumnType("text")
            .HasColumnOrder(23)
            .HasComment("Ý kiến cuối cùng");
        builder.Property(x => x.CallbackEvent)
            .HasColumnName("callback_event")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(24)
            .HasComment("Event phát ra khi kết thúc để phân hệ cập nhật chứng từ");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(25)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(26)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(27)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(28)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(29)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(30)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(31)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.DocumentTypeCode, x.DocumentId }, "ix_wf_instances_document");
        builder.HasIndex(x => new { x.RequesterId, x.SubmittedAt }, "ix_wf_instances_requester")
            .IsDescending(false, true);
        builder.HasIndex(x => new { x.TenantId, x.Status, x.SubmittedAt }, "ix_wf_instances_status")
            .IsDescending(false, false, true);
        builder.HasIndex(x => new { x.DocumentTypeCode, x.DocumentId }, "uq_wf_instances_active_doc")
            .IsUnique()
            .HasFilter("status = 'PENDING'");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("instances_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Definition)
            .WithMany()
            .HasForeignKey(x => x.DefinitionId)
            .HasConstraintName("instances_definition_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.WorkflowDefinitionVersion)
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .HasConstraintName("instances_version_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("instances_document_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Requester)
            .WithMany()
            .HasForeignKey(x => x.RequesterId)
            .HasConstraintName("instances_requester_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.OnBehalfOf)
            .WithMany()
            .HasForeignKey(x => x.OnBehalfOfId)
            .HasConstraintName("instances_on_behalf_of_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("instances_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("instances_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("instances_department_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
