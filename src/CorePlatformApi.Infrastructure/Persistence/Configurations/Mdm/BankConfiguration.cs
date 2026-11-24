// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho bảng <c>banks</c>.
/// </summary>
public sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("banks", tb =>
        {
            tb.HasComment("Danh mục ngân hàng (dùng cho VietQR, chi lương, thanh toán NCC)");
        });

        builder.HasKey(x => x.Id).HasName("banks_pkey");

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
        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("char(2)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasDefaultValueSql("'VN'")
            .ValueGeneratedNever()
            .HasComment("Quốc gia");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã viết tắt (VCB, TCB, MB...)");
        builder.Property(x => x.Bin)
            .HasColumnName("bin")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .HasComment("Mã BIN NAPAS dùng tạo VietQR (970436...)");
        builder.Property(x => x.ShortName)
            .HasColumnName("short_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên ngắn (Vietcombank)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Tên đầy đủ");
        builder.Property(x => x.SwiftCode)
            .HasColumnName("swift_code")
            .HasColumnType("varchar(11)")
            .HasColumnOrder(7)
            .HasComment("Mã SWIFT/BIC");
        builder.Property(x => x.LogoUrl)
            .HasColumnName("logo_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Logo ngân hàng");
        builder.Property(x => x.SupportsVietqr)
            .HasColumnName("supports_vietqr")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Hỗ trợ VietQR hay không");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn sử dụng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Code, "banks_code_key").IsUnique();

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("banks_country_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
