// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Monitor;

/// <summary>
/// Cấu hình EF Core cho bảng <c>incidents</c>.
/// </summary>
public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents", tb =>
        {
            tb.HasComment("Sự cố (downtime) mở/đóng tự động từ health check, phục vụ báo cáo uptime");
            tb.HasCheckConstraint("incidents_severity_check", "severity IN ('MINOR','MAJOR','CRITICAL')");
        });

        builder.HasKey(x => x.Id).HasName("incidents_pkey");

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
        builder.Property(x => x.ServiceId)
            .HasColumnName("service_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Service");
        builder.Property(x => x.Severity)
            .HasColumnName("severity")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'MAJOR'")
            .ValueGeneratedNever()
            .HasComment("Mức độ");
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tiêu đề");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Bắt đầu sự cố");
        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Khắc phục lúc");
        builder.Property(x => x.RootCause)
            .HasColumnName("root_cause")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Nguyên nhân");
        builder.Property(x => x.Resolution)
            .HasColumnName("resolution")
            .HasColumnType("text")
            .HasColumnOrder(8)
            .HasComment("Cách xử lý");
        builder.Property(x => x.AcknowledgedBy)
            .HasColumnName("acknowledged_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tiếp nhận");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người xóa mềm (users.id)");


        builder.HasOne(x => x.Service)
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .HasConstraintName("incidents_service_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
