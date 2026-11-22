// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Cấu hình EF Core cho <c>audit.audit_policies</c>.
/// </summary>
public sealed class AuditPolicyConfiguration : IEntityTypeConfiguration<AuditPolicy>
{
    public void Configure(EntityTypeBuilder<AuditPolicy> builder)
    {
        builder.ToTable("audit_policies", "audit", tb =>
        {
            tb.HasComment("Cấu hình đối tượng cần lưu vết thay đổi và trường cần che (lương, CCCD...)");
        });

        builder.HasKey(x => x.Id).HasName("audit_policies_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Phân hệ");
        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại đối tượng (EMPLOYEE, SALARY, STOCK_ADJUSTMENT)");
        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bật lưu vết");
        builder.Property(x => x.TrackActions)
            .HasColumnName("track_actions")
            .HasColumnType("text[]")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'{INSERT,UPDATE,DELETE}'")
            .ValueGeneratedNever()
            .HasComment("Hành động cần ghi");
        builder.Property(x => x.IncludeFields)
            .HasColumnName("include_fields")
            .HasColumnType("text[]")
            .HasColumnOrder(5)
            .HasComment("Chỉ ghi các trường này (NULL = tất cả)");
        builder.Property(x => x.ExcludeFields)
            .HasColumnName("exclude_fields")
            .HasColumnType("text[]")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'{updated_at,updated_by}'")
            .ValueGeneratedNever()
            .HasComment("Bỏ qua trường này");
        builder.Property(x => x.MaskedFields)
            .HasColumnName("masked_fields")
            .HasColumnType("text[]")
            .HasColumnOrder(7)
            .HasComment("Trường được ghi nhưng che giá trị (***), VD: base_salary, id_number");
        builder.Property(x => x.RetentionDays)
            .HasColumnName("retention_days")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("3650")
            .ValueGeneratedNever()
            .HasComment("Thời gian lưu (ngày) — chứng từ kế toán tối thiểu 10 năm");
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

        builder.HasIndex(x => new { x.ModuleCode, x.EntityType }, "uq_audit_policies").IsUnique();
    }
}
