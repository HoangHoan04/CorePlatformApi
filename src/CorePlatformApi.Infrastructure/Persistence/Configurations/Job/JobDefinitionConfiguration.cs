// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Job;

/// <summary>
/// Cấu hình EF Core cho bảng <c>job_definitions</c>.
/// </summary>
public sealed class JobDefinitionConfiguration : IEntityTypeConfiguration<JobDefinition>
{
    public void Configure(EntityTypeBuilder<JobDefinition> builder)
    {
        builder.ToTable("job_definitions", tb =>
        {
            tb.HasComment("Danh mục job định kỳ toàn hệ thống (chốt công cuối ngày, cảnh báo tồn kho, dọn file rác, đồng bộ tỷ giá...)");
        });

        builder.HasKey(x => x.Id).HasName("job_definitions_pkey");

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
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã job (core.storage.cleanup_temp_files)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ sở hữu");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.Handler)
            .HasColumnName("handler")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Tên class/handler thực thi (Assembly.Namespace.JobClass)");
        builder.Property(x => x.TargetService)
            .HasColumnName("target_service")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(7)
            .HasComment("Service chạy job (hrm-worker, core-worker)");
        builder.Property(x => x.CronExpression)
            .HasColumnName("cron_expression")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(8)
            .HasComment("Lịch cron (NULL = chỉ chạy tay)");
        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'Asia/Ho_Chi_Minh'")
            .ValueGeneratedNever()
            .HasComment("Múi giờ tính cron");
        builder.Property(x => x.RunPerTenant)
            .HasColumnName("run_per_tenant")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Chạy lặp qua từng tenant");
        builder.Property(x => x.Parameters)
            .HasColumnName("parameters")
            .HasColumnType("jsonb")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Tham số mặc định");
        builder.Property(x => x.QueueName)
            .HasColumnName("queue_name")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("'default'")
            .ValueGeneratedNever()
            .HasComment("Hàng đợi");
        builder.Property(x => x.TimeoutSeconds)
            .HasColumnName("timeout_seconds")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("3600")
            .ValueGeneratedNever()
            .HasComment("Thời gian chạy tối đa");
        builder.Property(x => x.MaxRetries)
            .HasColumnName("max_retries")
            .HasColumnType("smallint")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("3")
            .ValueGeneratedNever()
            .HasComment("Số lần retry khi lỗi");
        builder.Property(x => x.AllowConcurrent)
            .HasColumnName("allow_concurrent")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho phép chạy chồng");
        builder.Property(x => x.NotifyOnFailure)
            .HasColumnName("notify_on_failure")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Gửi cảnh báo khi lỗi");
        builder.Property(x => x.NotifyUserIds)
            .HasColumnName("notify_user_ids")
            .HasColumnType("uuid[]")
            .HasColumnOrder(17)
            .HasComment("Người nhận cảnh báo");
        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang bật");
        builder.Property(x => x.LastRunAt)
            .HasColumnName("last_run_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .HasComment("Lần chạy gần nhất");
        builder.Property(x => x.LastStatus)
            .HasColumnName("last_status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(20)
            .HasComment("Kết quả lần gần nhất");
        builder.Property(x => x.NextRunAt)
            .HasColumnName("next_run_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Lần chạy kế tiếp (tham khảo)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(25)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(26)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(27)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(28)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Code, "job_definitions_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("job_definitions_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
