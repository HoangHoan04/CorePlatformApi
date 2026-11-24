// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Job;

/// <summary>
/// Cấu hình EF Core cho bảng <c>executions</c>.
/// </summary>
public sealed class JobExecutionConfiguration : IEntityTypeConfiguration<JobExecution>
{
    public void Configure(EntityTypeBuilder<JobExecution> builder)
    {
        builder.ToTable("executions", tb =>
        {
            tb.HasComment("Lịch sử từng lần chạy job");
            tb.HasCheckConstraint("executions_trigger_type_check", "trigger_type IN ('SCHEDULED','MANUAL','RETRY','EVENT','API')");
            tb.HasCheckConstraint("executions_status_check", "status IN ('QUEUED','RUNNING','SUCCEEDED','FAILED','CANCELLED','TIMEOUT','SKIPPED')");
        });

        builder.HasKey(x => x.Id).HasName("executions_pkey");

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
            .HasComment("Tenant (job chạy theo tenant)");
        builder.Property(x => x.JobId)
            .HasColumnName("job_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Job");
        builder.Property(x => x.TriggerType)
            .HasColumnName("trigger_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'SCHEDULED'")
            .ValueGeneratedNever()
            .HasComment("Nguồn kích hoạt");
        builder.Property(x => x.TriggeredBy)
            .HasColumnName("triggered_by")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("User chạy tay");
        builder.Property(x => x.EngineJobId)
            .HasColumnName("engine_job_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(5)
            .HasComment("Id job bên Hangfire/Quartz");
        builder.Property(x => x.WorkerNode)
            .HasColumnName("worker_node")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(6)
            .HasComment("Máy/container thực thi");
        builder.Property(x => x.Parameters)
            .HasColumnName("parameters")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .HasComment("Tham số thực tế");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("'RUNNING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.ProgressPercent)
            .HasColumnName("progress_percent")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .HasComment("Tiến độ %");
        builder.Property(x => x.ProcessedCount)
            .HasColumnName("processed_count")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .HasComment("Số bản ghi đã xử lý");
        builder.Property(x => x.FailedCount)
            .HasColumnName("failed_count")
            .HasColumnType("int")
            .HasColumnOrder(11)
            .HasComment("Số bản ghi lỗi");
        builder.Property(x => x.ResultSummary)
            .HasColumnName("result_summary")
            .HasColumnType("text")
            .HasColumnOrder(12)
            .HasComment("Tóm tắt kết quả");
        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnType("text")
            .HasColumnOrder(13)
            .HasComment("Lỗi");
        builder.Property(x => x.ErrorStack)
            .HasColumnName("error_stack")
            .HasColumnType("text")
            .HasColumnOrder(14)
            .HasComment("Stack trace");
        builder.Property(x => x.AttemptNo)
            .HasColumnName("attempt_no")
            .HasColumnType("smallint")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Lần thử");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Bắt đầu");
        builder.Property(x => x.FinishedAt)
            .HasColumnName("finished_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Kết thúc");
        builder.Property(x => x.DurationMs)
            .HasColumnName("duration_ms")
            .HasColumnType("int")
            .HasColumnOrder(18)
            .HasComment("Thời gian chạy");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(23)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(25)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.JobId, x.StartedAt }, "ix_job_executions_job")
            .IsDescending(false, true);
        builder.HasIndex(x => new { x.Status, x.StartedAt }, "ix_job_executions_status")
            .IsDescending(false, true);

        builder.HasOne(x => x.Job)
            .WithMany(p => p.JobExecutions)
            .HasForeignKey(x => x.JobId)
            .HasConstraintName("executions_job_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
