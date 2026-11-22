// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.exchange_rates</c>.
/// </summary>
public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates", "mdm", tb =>
        {
            tb.HasComment("Tỷ giá quy đổi theo thời điểm");
            tb.HasCheckConstraint("exchange_rates_rate_type_check", "rate_type IN ('BUY_CASH','BUY_TRANSFER','SELL','TRANSFER','CENTRAL','ACCOUNTING')");
            tb.HasCheckConstraint("exchange_rates_rate_check", "rate > 0");
            tb.HasCheckConstraint("ck_exchange_rate_pair", "from_currency <> to_currency");
        });

        builder.HasKey(x => x.Id).HasName("exchange_rates_pkey");

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
            .HasComment("Tenant (NULL = tỷ giá hệ thống đồng bộ tự động)");
        builder.Property(x => x.FromCurrency)
            .HasColumnName("from_currency")
            .HasColumnType("char(3)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tiền tệ nguồn (USD)");
        builder.Property(x => x.ToCurrency)
            .HasColumnName("to_currency")
            .HasColumnType("char(3)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tiền tệ đích (VND)");
        builder.Property(x => x.RateType)
            .HasColumnName("rate_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'TRANSFER'")
            .ValueGeneratedNever()
            .HasComment("Loại tỷ giá: mua TM, mua CK, bán, chuyển khoản, trung tâm NHNN, hạch toán");
        builder.Property(x => x.Rate)
            .HasColumnName("rate")
            .HasColumnType("numeric(20,8)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tỷ giá: 1 from_currency = rate to_currency");
        builder.Property(x => x.EffectiveAt)
            .HasColumnName("effective_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Thời điểm áp dụng");
        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'MANUAL'")
            .ValueGeneratedNever()
            .HasComment("Nguồn: VCB, SBV, MANUAL...");
        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(8)
            .HasComment("Ghi chú");
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

        builder.HasIndex(x => new { x.TenantId, x.FromCurrency, x.ToCurrency, x.RateType, x.EffectiveAt }, "uq_exchange_rates")
            .IsUnique()
            .AreNullsDistinct(false);
        builder.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.RateType, x.EffectiveAt }, "ix_exchange_rates_lookup")
            .IsDescending(false, false, false, true);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("exchange_rates_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.FromCurrencyNavigation)
            .WithMany()
            .HasForeignKey(x => x.FromCurrency)
            .HasConstraintName("exchange_rates_from_currency_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ToCurrencyNavigation)
            .WithMany()
            .HasForeignKey(x => x.ToCurrency)
            .HasConstraintName("exchange_rates_to_currency_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
