// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Storage;
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.tasks</c>.
/// </summary>
public sealed class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.ToTable("tasks", "workflow", tb =>
        {
            tb.HasComment("Nhiệm vụ duyệt giao cho từng người (danh sách \"Việc chờ tôi duyệt\")");
            tb.HasCheckConstraint("tasks_task_type_check", "task_type IN ('APPROVE','CONSULT','ACKNOWLEDGE')");
            tb.HasCheckConstraint("tasks_status_check", "status IN ('WAITING','PENDING','APPROVED','REJECTED','RETURNED','DELEGATED','REASSIGNED','CANCELLED','EXPIRED','SKIPPED')");
            tb.HasCheckConstraint("tasks_acted_via_check", "acted_via IN ('WEB','MOBILE','EMAIL','API','SYSTEM')");
        });

        builder.HasKey(x => x.Id).HasName("tasks_pkey");

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
        builder.Property(x => x.InstanceId)
            .HasColumnName("instance_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Instance");
        builder.Property(x => x.InstanceStepId)
            .HasColumnName("instance_step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Lượt bước");
        builder.Property(x => x.AssigneeId)
            .HasColumnName("assignee_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Người đang phải xử lý");
        builder.Property(x => x.OriginalAssigneeId)
            .HasColumnName("original_assignee_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Người được giao gốc (khác assignee khi ủy quyền/chuyển)");
        builder.Property(x => x.DelegationId)
            .HasColumnName("delegation_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Ủy quyền đã áp dụng (workflow.delegations)");
        builder.Property(x => x.TaskType)
            .HasColumnName("task_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'APPROVE'")
            .ValueGeneratedNever()
            .HasComment("Duyệt, xin ý kiến, chỉ xem/biết");
        builder.Property(x => x.SequenceNo)
            .HasColumnName("sequence_no")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Thứ tự khi duyệt tuần tự");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasColumnType("text")
            .HasColumnOrder(10)
            .HasComment("Ý kiến khi xử lý");
        builder.Property(x => x.SignatureFileId)
            .HasColumnName("signature_file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Ảnh chữ ký");
        builder.Property(x => x.DueAt)
            .HasColumnName("due_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Hạn xử lý");
        builder.Property(x => x.ReminderCount)
            .HasColumnName("reminder_count")
            .HasColumnType("smallint")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần đã nhắc");
        builder.Property(x => x.LastRemindedAt)
            .HasColumnName("last_reminded_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Nhắc gần nhất");
        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Đã xem lúc");
        builder.Property(x => x.ActedAt)
            .HasColumnName("acted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Xử lý lúc");
        builder.Property(x => x.ActedIp)
            .HasColumnName("acted_ip")
            .HasColumnType("inet")
            .HasColumnOrder(17)
            .HasComment("IP khi xử lý");
        builder.Property(x => x.ActedVia)
            .HasColumnName("acted_via")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(18)
            .HasComment("Kênh xử lý");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Giao lúc");

        builder.HasIndex(x => new { x.AssigneeId, x.CreatedAt }, "ix_wf_tasks_inbox")
            .IsDescending(false, true)
            .HasFilter("status = 'PENDING'");
        builder.HasIndex(x => x.InstanceId, "ix_wf_tasks_instance");
        builder.HasIndex(x => x.DueAt, "ix_wf_tasks_due")
            .HasFilter("status = 'PENDING'");

        builder.HasOne(x => x.Instance)
            .WithMany(p => p.WorkflowTasks)
            .HasForeignKey(x => x.InstanceId)
            .HasConstraintName("tasks_instance_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.InstanceStep)
            .WithMany(p => p.WorkflowTasks)
            .HasForeignKey(x => x.InstanceStepId)
            .HasConstraintName("tasks_instance_step_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Assignee)
            .WithMany()
            .HasForeignKey(x => x.AssigneeId)
            .HasConstraintName("tasks_assignee_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.OriginalAssignee)
            .WithMany()
            .HasForeignKey(x => x.OriginalAssigneeId)
            .HasConstraintName("tasks_original_assignee_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SignatureFile)
            .WithMany()
            .HasForeignKey(x => x.SignatureFileId)
            .HasConstraintName("tasks_signature_file_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Delegation)
            .WithMany()
            .HasForeignKey(x => x.DelegationId)
            .HasConstraintName("fk_wf_tasks_delegation")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
