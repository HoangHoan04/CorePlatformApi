// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Job;

/// <summary>
/// Cấu hình EF Core cho <c>job.definitions</c>.
/// </summary>
public sealed class JobDefinitionConfiguration : IEntityTypeConfiguration<JobDefinition>
{
    public void Configure(EntityTypeBuilder<JobDefinition> builder)
    {
        builder.ToTable("definitions", "job", tb =>
        {
            tb.HasComment("Danh mục job định kỳ toàn hệ thống (chốt công cuối ngày, cảnh báo tồn kho, dọn file rác, đồng bộ tỷ giá...)");
        });

        builder.HasKey(x => x.Id).HasName("definitions_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã job (core.storage.cleanup_temp_files)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Phân hệ sở hữu");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .HasComment("Mô tả");
        builder.Property(x => x.Handler)
            .HasColumnName("handler")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên class/handler thực thi (Assembly.Namespace.JobClass)");
        builder.Property(x => x.TargetService)
            .HasColumnName("target_service")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(6)
            .HasComment("Service chạy job (hrm-worker, core-worker)");
        builder.Property(x => x.CronExpression)
            .HasColumnName("cron_expression")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(7)
            .HasComment("Lịch cron (NULL = chỉ chạy tay)");
        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("'Asia/Ho_Chi_Minh'")
            .ValueGeneratedNever()
            .HasComment("Múi giờ tính cron");
        builder.Property(x => x.RunPerTenant)
            .HasColumnName("run_per_tenant")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Chạy lặp qua từng tenant");
        builder.Property(x => x.Parameters)
            .HasColumnName("parameters")
            .HasColumnType("jsonb")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Tham số mặc định");
        builder.Property(x => x.QueueName)
            .HasColumnName("queue_name")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'default'")
            .ValueGeneratedNever()
            .HasComment("Hàng đợi");
        builder.Property(x => x.TimeoutSeconds)
            .HasColumnName("timeout_seconds")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("3600")
            .ValueGeneratedNever()
            .HasComment("Thời gian chạy tối đa");
        builder.Property(x => x.MaxRetries)
            .HasColumnName("max_retries")
            .HasColumnType("smallint")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("3")
            .ValueGeneratedNever()
            .HasComment("Số lần retry khi lỗi");
        builder.Property(x => x.AllowConcurrent)
            .HasColumnName("allow_concurrent")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho phép chạy chồng");
        builder.Property(x => x.NotifyOnFailure)
            .HasColumnName("notify_on_failure")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Gửi cảnh báo khi lỗi");
        builder.Property(x => x.NotifyUserIds)
            .HasColumnName("notify_user_ids")
            .HasColumnType("uuid[]")
            .HasColumnOrder(16)
            .HasComment("Người nhận cảnh báo");
        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang bật");
        builder.Property(x => x.LastRunAt)
            .HasColumnName("last_run_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Lần chạy gần nhất");
        builder.Property(x => x.LastStatus)
            .HasColumnName("last_status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(19)
            .HasComment("Kết quả lần gần nhất");
        builder.Property(x => x.NextRunAt)
            .HasColumnName("next_run_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Lần chạy kế tiếp (tham khảo)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.Code, "definitions_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("definitions_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
