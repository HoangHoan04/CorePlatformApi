// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho bảng <c>instance_steps</c>.
/// </summary>
public sealed class WorkflowInstanceStepConfiguration : IEntityTypeConfiguration<WorkflowInstanceStep>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceStep> builder)
    {
        builder.ToTable("instance_steps", tb =>
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
        builder.Property(x => x.StepId)
            .HasColumnName("step_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Bước định nghĩa");
        builder.Property(x => x.Iteration)
            .HasColumnName("iteration")
            .HasColumnType("smallint")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Lần chạy thứ mấy của bước (tăng khi bị trả lại)");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'IN_PROGRESS'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái bước");
        builder.Property(x => x.Outcome)
            .HasColumnName("outcome")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(6)
            .HasComment("Kết quả để chọn transition");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Bắt đầu");
        builder.Property(x => x.DueAt)
            .HasColumnName("due_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Hạn xử lý (đã tính theo lịch làm việc)");
        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Hoàn thành");
        builder.Property(x => x.EscalatedAt)
            .HasColumnName("escalated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Đã leo thang lúc");
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
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (users.id)");
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
