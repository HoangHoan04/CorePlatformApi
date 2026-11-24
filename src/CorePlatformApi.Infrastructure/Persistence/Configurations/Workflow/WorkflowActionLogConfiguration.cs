// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho bảng <c>action_logs</c>.
/// </summary>
public sealed class WorkflowActionLogConfiguration : IEntityTypeConfiguration<WorkflowActionLog>
{
    public void Configure(EntityTypeBuilder<WorkflowActionLog> builder)
    {
        builder.ToTable("action_logs", tb =>
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
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.InstanceId)
            .HasColumnName("instance_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Instance");
        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Nhiệm vụ liên quan");
        builder.Property(x => x.InstanceStepId)
            .HasColumnName("instance_step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Lượt bước");
        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Hành động");
        builder.Property(x => x.ActorId)
            .HasColumnName("actor_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Người thao tác (NULL = hệ thống)");
        builder.Property(x => x.OnBehalfOfId)
            .HasColumnName("on_behalf_of_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Thao tác thay cho ai (ủy quyền)");
        builder.Property(x => x.TargetUserId)
            .HasColumnName("target_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người nhận (khi chuyển/ủy quyền/xin ý kiến)");
        builder.Property(x => x.FromStepName)
            .HasColumnName("from_step_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(9)
            .HasComment("Bước trước");
        builder.Property(x => x.ToStepName)
            .HasColumnName("to_step_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(10)
            .HasComment("Bước sau");
        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasColumnType("text")
            .HasColumnOrder(11)
            .HasComment("Ý kiến");
        builder.Property(x => x.Attachments)
            .HasColumnName("attachments")
            .HasColumnType("uuid[]")
            .HasColumnOrder(12)
            .HasComment("File đính kèm (storage.files)");
        builder.Property(x => x.DataChanges)
            .HasColumnName("data_changes")
            .HasColumnType("jsonb")
            .HasColumnOrder(13)
            .HasComment("Dữ liệu chứng từ được người duyệt sửa");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(14)
            .HasComment("IP");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người xóa mềm (users.id)");

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
