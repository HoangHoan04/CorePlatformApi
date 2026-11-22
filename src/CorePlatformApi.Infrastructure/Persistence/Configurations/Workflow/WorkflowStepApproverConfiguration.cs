// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.step_approvers</c>.
/// </summary>
public sealed class WorkflowStepApproverConfiguration : IEntityTypeConfiguration<WorkflowStepApprover>
{
    public void Configure(EntityTypeBuilder<WorkflowStepApprover> builder)
    {
        builder.ToTable("step_approvers", "workflow", tb =>
        {
            tb.HasComment("Người duyệt của một bước: chỉ định cụ thể, theo vai trò, quản lý trực tiếp, trưởng phòng, cấp quản lý thứ N...");
            tb.HasCheckConstraint("step_approvers_approver_type_check", "approver_type IN ('USER','ROLE','DIRECT_MANAGER','MANAGER_LEVEL','DEPARTMENT_MANAGER','BRANCH_MANAGER','COMPANY_DIRECTOR','REQUESTER','FIELD_VALUE','EXPRESSION')");
            tb.HasCheckConstraint("step_approvers_scope_by_check", "scope_by IN ('REQUESTER','DOCUMENT')");
        });

        builder.HasKey(x => x.Id).HasName("step_approvers_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.StepId)
            .HasColumnName("step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Bước");
        builder.Property(x => x.ApproverType)
            .HasColumnName("approver_type")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Cách xác định người duyệt");
        builder.Property(x => x.ApproverRefId)
            .HasColumnName("approver_ref_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Id user / role / phòng ban tương ứng");
        builder.Property(x => x.ManagerLevel)
            .HasColumnName("manager_level")
            .HasColumnType("smallint")
            .HasColumnOrder(4)
            .HasComment("Cấp quản lý thứ N (MANAGER_LEVEL)");
        builder.Property(x => x.FieldName)
            .HasColumnName("field_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(5)
            .HasComment("Lấy người duyệt từ trường của chứng từ (FIELD_VALUE)");
        builder.Property(x => x.Expression)
            .HasColumnName("expression")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Biểu thức tùy biến (EXPRESSION)");
        builder.Property(x => x.ScopeBy)
            .HasColumnName("scope_by")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .HasComment("ROLE: lọc người có vai trò trong đơn vị của người tạo hay của chứng từ");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự khi duyệt SEQUENTIAL");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");


        builder.HasOne(x => x.Step)
            .WithMany(p => p.WorkflowStepApprovers)
            .HasForeignKey(x => x.StepId)
            .HasConstraintName("step_approvers_step_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
