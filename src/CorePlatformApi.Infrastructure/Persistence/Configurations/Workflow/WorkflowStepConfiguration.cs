// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.steps</c>.
/// </summary>
public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("steps", "workflow", tb =>
        {
            tb.HasComment("Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc");
            tb.HasCheckConstraint("steps_step_type_check", "step_type IN ('START','APPROVAL','CONDITION','PARALLEL_SPLIT','PARALLEL_JOIN','NOTIFY','SYSTEM','END')");
            tb.HasCheckConstraint("steps_approval_mode_check", "approval_mode IN ('ANY','ALL','SEQUENTIAL','QUORUM')");
            tb.HasCheckConstraint("steps_quorum_percent_check", "quorum_percent BETWEEN 1 AND 100");
            tb.HasCheckConstraint("steps_timeout_action_check", "timeout_action IN ('NONE','REMIND','ESCALATE','AUTO_APPROVE','AUTO_REJECT')");
        });

        builder.HasKey(x => x.Id).HasName("steps_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.VersionId)
            .HasColumnName("version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Phiên bản");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã bước (trong phiên bản)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên bước (Trưởng bộ phận duyệt)");
        builder.Property(x => x.StepType)
            .HasColumnName("step_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Loại node");
        builder.Property(x => x.ApprovalMode)
            .HasColumnName("approval_mode")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .HasComment("Cách duyệt khi nhiều người: 1 người bất kỳ, tất cả, lần lượt, đủ tỷ lệ");
        builder.Property(x => x.QuorumPercent)
            .HasColumnName("quorum_percent")
            .HasColumnType("smallint")
            .HasColumnOrder(6)
            .HasComment("Tỷ lệ % cần đồng ý (QUORUM)");
        builder.Property(x => x.SlaHours)
            .HasColumnName("sla_hours")
            .HasColumnType("numeric(8,2)")
            .HasColumnOrder(7)
            .HasComment("Hạn xử lý (giờ)");
        builder.Property(x => x.SlaUseWorkingCalendar)
            .HasColumnName("sla_use_working_calendar")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Tính hạn theo giờ làm việc (mdm.work_calendars), bỏ qua lễ/CN");
        builder.Property(x => x.RemindBeforeHours)
            .HasColumnName("remind_before_hours")
            .HasColumnType("numeric(8,2)")
            .HasColumnOrder(9)
            .HasComment("Nhắc trước hạn N giờ");
        builder.Property(x => x.RemindIntervalHours)
            .HasColumnName("remind_interval_hours")
            .HasColumnType("numeric(8,2)")
            .HasColumnOrder(10)
            .HasComment("Nhắc lặp lại mỗi N giờ khi quá hạn");
        builder.Property(x => x.MaxReminders)
            .HasColumnName("max_reminders")
            .HasColumnType("smallint")
            .HasColumnOrder(11)
            .HasDefaultValueSql("3")
            .ValueGeneratedNever()
            .HasComment("Số lần nhắc tối đa");
        builder.Property(x => x.TimeoutAction)
            .HasColumnName("timeout_action")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("'REMIND'")
            .ValueGeneratedNever()
            .HasComment("Hành động khi quá hạn");
        builder.Property(x => x.EscalateToType)
            .HasColumnName("escalate_to_type")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(13)
            .HasComment("Loại người nhận khi leo thang (giống approver_type)");
        builder.Property(x => x.EscalateToRef)
            .HasColumnName("escalate_to_ref")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Id người/vai trò nhận leo thang");
        builder.Property(x => x.AllowReject)
            .HasColumnName("allow_reject")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Cho phép từ chối");
        builder.Property(x => x.AllowReturn)
            .HasColumnName("allow_return")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Cho phép trả lại bước trước/người tạo");
        builder.Property(x => x.AllowDelegate)
            .HasColumnName("allow_delegate")
            .HasColumnType("boolean")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Cho phép chuyển/ủy quyền");
        builder.Property(x => x.AllowAddApprover)
            .HasColumnName("allow_add_approver")
            .HasColumnType("boolean")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho phép xin ý kiến thêm người");
        builder.Property(x => x.RequireCommentOnReject)
            .HasColumnName("require_comment_on_reject")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc ghi lý do khi từ chối");
        builder.Property(x => x.RequireSignature)
            .HasColumnName("require_signature")
            .HasColumnType("boolean")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Yêu cầu chữ ký (ảnh/số)");
        builder.Property(x => x.EditableFields)
            .HasColumnName("editable_fields")
            .HasColumnType("text[]")
            .HasColumnOrder(21)
            .HasComment("Trường chứng từ người duyệt bước này được sửa");
        builder.Property(x => x.OnEnterActions)
            .HasColumnName("on_enter_actions")
            .HasColumnType("jsonb")
            .HasColumnOrder(22)
            .HasComment("Hành động hệ thống khi vào bước (gọi webhook, phát event)");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(23)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(25)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(26)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(27)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.VersionId, x.Code }, "uq_wf_steps").IsUnique();

        builder.HasOne(x => x.Version)
            .WithMany(p => p.WorkflowSteps)
            .HasForeignKey(x => x.VersionId)
            .HasConstraintName("steps_version_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
