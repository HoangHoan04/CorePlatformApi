// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.numbering_counters</c>.
/// </summary>
public sealed class NumberingCounterConfiguration : IEntityTypeConfiguration<NumberingCounter>
{
    public void Configure(EntityTypeBuilder<NumberingCounter> builder)
    {
        builder.ToTable("numbering_counters", "mdm", tb =>
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
        builder.Property(x => x.RuleId)
            .HasColumnName("rule_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Quy tắc");
        builder.Property(x => x.PeriodKey)
            .HasColumnName("period_key")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Khóa kỳ: 'ALL', '2026', '202611', '20261121'");
        builder.Property(x => x.ScopeKey)
            .HasColumnName("scope_key")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'-'")
            .ValueGeneratedNever()
            .HasComment("Khóa phạm vi: '-' hoặc id công ty/chi nhánh");
        builder.Property(x => x.CurrentValue)
            .HasColumnName("current_value")
            .HasColumnType("bigint")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Giá trị đã cấp gần nhất");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần cấp gần nhất");

        builder.HasIndex(x => new { x.RuleId, x.PeriodKey, x.ScopeKey }, "uq_numbering_counters").IsUnique();

        builder.HasOne(x => x.Rule)
            .WithMany(p => p.NumberingCounters)
            .HasForeignKey(x => x.RuleId)
            .HasConstraintName("numbering_counters_rule_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
