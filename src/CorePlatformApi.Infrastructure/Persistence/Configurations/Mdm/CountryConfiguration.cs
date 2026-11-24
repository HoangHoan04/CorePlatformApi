// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>countries</c>.
/// </summary>
public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries", tb =>
        {
            tb.HasComment("Danh mục quốc gia theo chuẩn ISO 3166");
        });

        builder.HasKey(x => x.Id).HasName("countries_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (UUID v7)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("char(2)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã ISO 3166-1 alpha-2 (VN, US, JP...)");
        builder.Property(x => x.Alpha3Code)
            .HasColumnName("alpha3_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã ISO alpha-3 (VNM, USA...)");
        builder.Property(x => x.NumericCode)
            .HasColumnName("numeric_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(4)
            .HasComment("Mã số ISO (704 = Việt Nam)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên tiếng Việt");
        builder.Property(x => x.NameEn)
            .HasColumnName("name_en")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Tên tiếng Anh");
        builder.Property(x => x.PhoneCode)
            .HasColumnName("phone_code")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .HasComment("Mã vùng điện thoại quốc tế (+84)");
        builder.Property(x => x.CurrencyCode)
            .HasColumnName("currency_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(8)
            .HasComment("Tiền tệ mặc định của quốc gia (VND)");
        builder.Property(x => x.FlagEmoji)
            .HasColumnName("flag_emoji")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(9)
            .HasComment("Cờ dạng emoji để hiển thị nhanh");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn sử dụng hay không");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Alpha3Code, "countries_alpha3_code_key").IsUnique();
        builder.HasAlternateKey(x => x.Code).HasName("countries_code_key");
    }
}
