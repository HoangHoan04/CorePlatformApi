// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>numbering_counters</c>.
/// </summary>
public sealed class NumberingCounterConfiguration : IEntityTypeConfiguration<NumberingCounter>
{
    public void Configure(EntityTypeBuilder<NumberingCounter> builder)
    {
        builder.ToTable("numbering_counters", tb =>
        {
            tb.HasComment("Bộ đếm hiện tại của từng quy tắc theo kỳ (năm/tháng/ngày) và phạm vi (công ty/chi nhánh)");
        });

        builder.HasKey(x => x.Id).HasName("numbering_counters_pkey");

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
        builder.Property(x => x.RuleId)
            .HasColumnName("rule_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Quy tắc");
        builder.Property(x => x.PeriodKey)
            .HasColumnName("period_key")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Khóa kỳ: 'ALL', '2026', '202611', '20261121'");
        builder.Property(x => x.ScopeKey)
            .HasColumnName("scope_key")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'-'")
            .ValueGeneratedNever()
            .HasComment("Khóa phạm vi: '-' hoặc id công ty/chi nhánh");
        builder.Property(x => x.CurrentValue)
            .HasColumnName("current_value")
            .HasColumnType("bigint")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Giá trị đã cấp gần nhất");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần cấp gần nhất");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.RuleId, x.PeriodKey, x.ScopeKey }, "uq_numbering_counters").IsUnique();

        builder.HasOne(x => x.Rule)
            .WithMany(p => p.NumberingCounters)
            .HasForeignKey(x => x.RuleId)
            .HasConstraintName("numbering_counters_rule_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
