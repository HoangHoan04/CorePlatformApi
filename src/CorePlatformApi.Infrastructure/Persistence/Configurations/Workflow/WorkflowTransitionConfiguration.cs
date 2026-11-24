// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho bảng <c>transitions</c>.
/// </summary>
public sealed class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.ToTable("transitions", tb =>
        {
            tb.HasComment("Cạnh nối giữa các bước kèm điều kiện rẽ nhánh (VD: amount < 10000000 → Trưởng BP; >= 50000000 → Giám đốc)");
            tb.HasCheckConstraint("transitions_trigger_outcome_check", "trigger_outcome IN ('APPROVED','REJECTED','RETURNED','ANY','TIMEOUT')");
        });

        builder.HasKey(x => x.Id).HasName("transitions_pkey");

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
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.VersionId)
            .HasColumnName("version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phiên bản");
        builder.Property(x => x.FromStepId)
            .HasColumnName("from_step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Bước nguồn");
        builder.Property(x => x.ToStepId)
            .HasColumnName("to_step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Bước đích");
        builder.Property(x => x.TriggerOutcome)
            .HasColumnName("trigger_outcome")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'APPROVED'")
            .ValueGeneratedNever()
            .HasComment("Kết quả bước nguồn kích hoạt cạnh này");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(6)
            .HasComment("Nhãn cạnh (Dưới 10 triệu)");
        builder.Property(x => x.ConditionJson)
            .HasColumnName("condition_json")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .HasComment("Điều kiện dạng cây JSON, VD: {\"all\":[{\"field\":\"amount\",\"op\":\"lt\",\"value\":10000000}]}");
        builder.Property(x => x.ConditionExpr)
            .HasColumnName("condition_expr")
            .HasColumnType("text")
            .HasColumnOrder(8)
            .HasComment("Điều kiện dạng biểu thức (thay thế condition_json), VD: amount >= 10000000 && amount < 50000000");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("int")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự xét điều kiện (nhỏ xét trước)");
        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Nhánh mặc định khi không điều kiện nào khớp");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.FromStepId, x.Priority }, "ix_wf_transitions_from");

        builder.HasOne(x => x.WorkflowDefinitionVersion)
            .WithMany(p => p.WorkflowTransitions)
            .HasForeignKey(x => x.VersionId)
            .HasConstraintName("transitions_version_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FromStep)
            .WithMany()
            .HasForeignKey(x => x.FromStepId)
            .HasConstraintName("transitions_from_step_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ToStep)
            .WithMany()
            .HasForeignKey(x => x.ToStepId)
            .HasConstraintName("transitions_to_step_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
