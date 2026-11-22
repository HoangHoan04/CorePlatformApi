// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.currencies</c>.
/// </summary>
public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies", "mdm", tb =>
        {
            tb.HasComment("Danh mục tiền tệ theo ISO 4217");
            tb.HasCheckConstraint("currencies_symbol_position_check", "symbol_position IN ('PREFIX','SUFFIX')");
        });

        builder.HasKey(x => x.Code).HasName("currencies_pkey");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("char(3)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Mã ISO 4217 (VND, USD, EUR...)");
        builder.Property(x => x.NumericCode)
            .HasColumnName("numeric_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(1)
            .HasComment("Mã số ISO (704 = VND)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên tiền tệ (Đồng Việt Nam)");
        builder.Property(x => x.Symbol)
            .HasColumnName("symbol")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Ký hiệu hiển thị (₫, $)");
        builder.Property(x => x.DecimalPlaces)
            .HasColumnName("decimal_places")
            .HasColumnType("smallint")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số chữ số thập phân khi hiển thị/làm tròn (VND = 0, USD = 2)");
        builder.Property(x => x.RoundingUnit)
            .HasColumnName("rounding_unit")
            .HasColumnType("numeric(18,6)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Đơn vị làm tròn tối thiểu (VD: 1, 100, 1000 VND)");
        builder.Property(x => x.SymbolPosition)
            .HasColumnName("symbol_position")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'SUFFIX'")
            .ValueGeneratedNever()
            .HasComment("Vị trí ký hiệu khi hiển thị");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn sử dụng");
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

    }
}
