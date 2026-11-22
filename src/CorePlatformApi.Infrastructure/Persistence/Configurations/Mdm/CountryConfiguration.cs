// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.countries</c>.
/// </summary>
public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries", "mdm", tb =>
        {
            tb.HasComment("Danh mục quốc gia theo chuẩn ISO 3166");
        });

        builder.HasKey(x => x.Code).HasName("countries_pkey");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("char(2)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Mã ISO 3166-1 alpha-2 (VN, US, JP...)");
        builder.Property(x => x.Alpha3Code)
            .HasColumnName("alpha3_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã ISO alpha-3 (VNM, USA...)");
        builder.Property(x => x.NumericCode)
            .HasColumnName("numeric_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(2)
            .HasComment("Mã số ISO (704 = Việt Nam)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên tiếng Việt");
        builder.Property(x => x.NameEn)
            .HasColumnName("name_en")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên tiếng Anh");
        builder.Property(x => x.PhoneCode)
            .HasColumnName("phone_code")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(5)
            .HasComment("Mã vùng điện thoại quốc tế (+84)");
        builder.Property(x => x.CurrencyCode)
            .HasColumnName("currency_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(6)
            .HasComment("Tiền tệ mặc định của quốc gia (VND)");
        builder.Property(x => x.FlagEmoji)
            .HasColumnName("flag_emoji")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .HasComment("Cờ dạng emoji để hiển thị nhanh");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn sử dụng hay không");
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

        builder.HasIndex(x => x.Alpha3Code, "countries_alpha3_code_key").IsUnique();
    }
}
