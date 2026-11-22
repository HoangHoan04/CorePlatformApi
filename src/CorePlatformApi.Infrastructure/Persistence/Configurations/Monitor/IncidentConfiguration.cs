// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Monitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Monitor;

/// <summary>
/// Cấu hình EF Core cho <c>monitor.incidents</c>.
/// </summary>
public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents", "monitor", tb =>
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
        builder.Property(x => x.ServiceId)
            .HasColumnName("service_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Service");
        builder.Property(x => x.Severity)
            .HasColumnName("severity")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasDefaultValueSql("'MAJOR'")
            .ValueGeneratedNever()
            .HasComment("Mức độ");
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tiêu đề");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Bắt đầu sự cố");
        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .HasComment("Khắc phục lúc");
        builder.Property(x => x.RootCause)
            .HasColumnName("root_cause")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Nguyên nhân");
        builder.Property(x => x.Resolution)
            .HasColumnName("resolution")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Cách xử lý");
        builder.Property(x => x.AcknowledgedBy)
            .HasColumnName("acknowledged_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người tiếp nhận");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");


        builder.HasOne(x => x.Service)
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .HasConstraintName("incidents_service_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
