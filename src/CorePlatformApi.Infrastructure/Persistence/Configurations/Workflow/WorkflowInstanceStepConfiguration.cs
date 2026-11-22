// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.instance_steps</c>.
/// </summary>
public sealed class WorkflowInstanceStepConfiguration : IEntityTypeConfiguration<WorkflowInstanceStep>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceStep> builder)
    {
        builder.ToTable("instance_steps", "workflow", tb =>
        {
            tb.HasComment("Lượt thực thi của từng bước trong instance (một bước có thể lặp lại khi bị trả về)");
            tb.HasCheckConstraint("instance_steps_status_check", "status IN ('WAITING','IN_PROGRESS','APPROVED','REJECTED','RETURNED','SKIPPED','TIMEOUT','CANCELLED')");
        });

        builder.HasKey(x => x.Id).HasName("instance_steps_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.InstanceId)
            .HasColumnName("instance_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Instance");
        builder.Property(x => x.StepId)
            .HasColumnName("step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Bước định nghĩa");
        builder.Property(x => x.Iteration)
            .HasColumnName("iteration")
            .HasColumnType("smallint")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Lần chạy thứ mấy của bước (tăng khi bị trả lại)");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'IN_PROGRESS'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái bước");
        builder.Property(x => x.Outcome)
            .HasColumnName("outcome")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .HasComment("Kết quả để chọn transition");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Bắt đầu");
        builder.Property(x => x.DueAt)
            .HasColumnName("due_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Hạn xử lý (đã tính theo lịch làm việc)");
        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Hoàn thành");
        builder.Property(x => x.EscalatedAt)
            .HasColumnName("escalated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Đã leo thang lúc");

        builder.HasIndex(x => x.InstanceId, "ix_wf_instance_steps_instance");
        builder.HasIndex(x => x.DueAt, "ix_wf_instance_steps_due")
            .HasFilter("status = 'IN_PROGRESS'");

        builder.HasOne(x => x.Instance)
            .WithMany(p => p.WorkflowInstanceSteps)
            .HasForeignKey(x => x.InstanceId)
            .HasConstraintName("instance_steps_instance_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Step)
            .WithMany()
            .HasForeignKey(x => x.StepId)
            .HasConstraintName("instance_steps_step_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
