// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.action_logs</c>.
/// </summary>
public sealed class WorkflowActionLogConfiguration : IEntityTypeConfiguration<WorkflowActionLog>
{
    public void Configure(EntityTypeBuilder<WorkflowActionLog> builder)
    {
        builder.ToTable("action_logs", "workflow", tb =>
        {
            tb.HasComment("Lịch sử thao tác trên instance (timeline duyệt)");
            tb.HasCheckConstraint("action_logs_action_check", "action IN ('SUBMIT','APPROVE','REJECT','RETURN','RECALL','CANCEL','DELEGATE','REASSIGN','CONSULT','COMMENT','REMIND','ESCALATE','AUTO_APPROVE','AUTO_REJECT','SKIP','RESUBMIT')");
        });

        builder.HasKey(x => x.Id).HasName("action_logs_pkey");

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
        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Nhiệm vụ liên quan");
        builder.Property(x => x.InstanceStepId)
            .HasColumnName("instance_step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Lượt bước");
        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Hành động");
        builder.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Người thao tác (NULL = hệ thống)");
        builder.Property(x => x.OnBehalfOfId)
            .HasColumnName("on_behalf_of_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Thao tác thay cho ai (ủy quyền)");
        builder.Property(x => x.TargetUserId)
            .HasColumnName("target_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người nhận (khi chuyển/ủy quyền/xin ý kiến)");
        builder.Property(x => x.FromStepName)
            .HasColumnName("from_step_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(8)
            .HasComment("Bước trước");
        builder.Property(x => x.ToStepName)
            .HasColumnName("to_step_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(9)
            .HasComment("Bước sau");
        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasColumnType("text")
            .HasColumnOrder(10)
            .HasComment("Ý kiến");
        builder.Property(x => x.Attachments)
            .HasColumnName("attachments")
            .HasColumnType("uuid[]")
            .HasColumnOrder(11)
            .HasComment("File đính kèm (storage.files)");
        builder.Property(x => x.DataChanges)
            .HasColumnName("data_changes")
            .HasColumnType("jsonb")
            .HasColumnOrder(12)
            .HasComment("Dữ liệu chứng từ được người duyệt sửa");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(13)
            .HasComment("IP");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");

        builder.HasIndex(x => new { x.InstanceId, x.CreatedAt }, "ix_wf_action_logs_instance");

        builder.HasOne(x => x.Instance)
            .WithMany(p => p.WorkflowActionLogs)
            .HasForeignKey(x => x.InstanceId)
            .HasConstraintName("action_logs_instance_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Task)
            .WithMany()
            .HasForeignKey(x => x.TaskId)
            .HasConstraintName("action_logs_task_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.InstanceStep)
            .WithMany()
            .HasForeignKey(x => x.InstanceStepId)
            .HasConstraintName("action_logs_instance_step_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
