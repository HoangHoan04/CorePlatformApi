// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Cấu hình EF Core cho <c>audit.data_change_logs</c>.
/// </summary>
public sealed class DataChangeLogConfiguration : IEntityTypeConfiguration<DataChangeLog>
{
    public void Configure(EntityTypeBuilder<DataChangeLog> builder)
    {
        builder.ToTable("data_change_logs", "audit", tb =>
        {
            tb.HasComment("Lịch sử thay đổi dữ liệu nhạy cảm (old/new value) phục vụ đối soát kế toán, kho, nhân sự. Các phân hệ gửi qua event hoặc ghi trực tiếp");
            tb.HasCheckConstraint("data_change_logs_action_check", "action IN ('INSERT','UPDATE','DELETE','RESTORE','APPROVE','EXPORT')");
            tb.HasCheckConstraint("data_change_logs_source_check", "source IN ('WEB','MOBILE','API','IMPORT','JOB','WORKFLOW','SYSTEM')");
            tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql
        });

        builder.HasKey(x => new { x.Id, x.ChangedAt }).HasName("data_change_logs_pkey");

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
            .HasComment("Tenant");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Công ty (lọc theo data scope)");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Phân hệ");
        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Loại đối tượng");
        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Id đối tượng (text để hỗ trợ khóa không phải uuid)");
        builder.Property(x => x.EntityDisplay)
            .HasColumnName("entity_display")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(6)
            .HasComment("Tên/mã hiển thị (NV-2026-0001 - Nguyễn Văn A)");
        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Hành động");
        builder.Property(x => x.ChangedFields)
            .HasColumnName("changed_fields")
            .HasColumnType("text[]")
            .HasColumnOrder(8)
            .HasComment("Danh sách trường thay đổi");
        builder.Property(x => x.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb")
            .HasColumnOrder(9)
            .HasComment("Giá trị cũ {field: value}");
        builder.Property(x => x.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb")
            .HasColumnOrder(10)
            .HasComment("Giá trị mới {field: value}");
        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(11)
            .HasComment("Lý do thay đổi (bắt buộc với một số nghiệp vụ)");
        builder.Property(x => x.ChangedBy)
            .HasColumnName("changed_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người thay đổi");
        builder.Property(x => x.ChangedByName)
            .HasColumnName("changed_by_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(13)
            .HasComment("Tên người thay đổi (snapshot)");
        builder.Property(x => x.OnBehalfOf)
            .HasColumnName("on_behalf_of")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Thay mặt (impersonation)");
        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'WEB'")
            .ValueGeneratedNever()
            .HasComment("Nguồn thay đổi");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(16)
            .HasComment("IP");
        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(17)
            .HasComment("Trace id liên kết request");
        builder.Property(x => x.ChangedAt)
            .HasColumnName("changed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");

    }
}
