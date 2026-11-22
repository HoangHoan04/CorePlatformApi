// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Job;

/// <summary>
/// Cấu hình EF Core cho <c>job.executions</c>.
/// </summary>
public sealed class JobExecutionConfiguration : IEntityTypeConfiguration<JobExecution>
{
    public void Configure(EntityTypeBuilder<JobExecution> builder)
    {
        builder.ToTable("executions", "job", tb =>
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
        builder.Property(x => x.JobId)
            .HasColumnName("job_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Job");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Tenant (job chạy theo tenant)");
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
