// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Org;

/// <summary>
/// Cấu hình EF Core cho bảng <c>tenants</c>.
/// </summary>
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", tb =>
        {
            tb.HasComment("Tenant = Tập đoàn/khách hàng thuê hệ thống. Mọi dữ liệu nghiệp vụ đều gắn tenant_id để cô lập dữ liệu");
            tb.HasCheckConstraint("tenants_status_check", "status IN ('TRIAL','ACTIVE','SUSPENDED','EXPIRED','CLOSED')");
            tb.HasCheckConstraint("tenants_isolation_mode_check", "isolation_mode IN ('SHARED','SCHEMA','DATABASE')");
        });

        builder.HasKey(x => x.Id).HasName("tenants_pkey");

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
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã tenant duy nhất, dùng trong subdomain/đăng nhập (VD: remak)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên hiển thị");
        builder.Property(x => x.LegalName)
            .HasColumnName("legal_name")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(4)
            .HasComment("Tên pháp lý đầy đủ");
        builder.Property(x => x.TaxCode)
            .HasColumnName("tax_code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .HasComment("Mã số thuế của tập đoàn/công ty mẹ");
        builder.Property(x => x.LogoFileId)
            .HasColumnName("logo_file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Logo (storage.files)");
        builder.Property(x => x.PrimaryDomain)
            .HasColumnName("primary_domain")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(7)
            .HasComment("Domain chính (erp.congty.vn)");
        builder.Property(x => x.Subdomain)
            .HasColumnName("subdomain")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(8)
            .HasComment("Subdomain trên nền tảng (remak.baseerp.vn)");
        builder.Property(x => x.ContactName)
            .HasColumnName("contact_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(9)
            .HasComment("Người liên hệ chính");
        builder.Property(x => x.ContactEmail)
            .HasColumnName("contact_email")
            .HasColumnType("citext")
            .HasColumnOrder(10)
            .HasComment("Email liên hệ");
        builder.Property(x => x.ContactPhone)
            .HasColumnName("contact_phone")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(11)
            .HasComment("Số điện thoại liên hệ");
        builder.Property(x => x.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("char(2)")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("'VN'")
            .ValueGeneratedNever()
            .HasComment("Quốc gia hoạt động chính");
        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("'Asia/Ho_Chi_Minh'")
            .ValueGeneratedNever()
            .HasComment("Múi giờ mặc định (IANA)");
        builder.Property(x => x.DefaultLocale)
            .HasColumnName("default_locale")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("'vi-VN'")
            .ValueGeneratedNever()
            .HasComment("Ngôn ngữ mặc định");
        builder.Property(x => x.DefaultCurrencyCode)
            .HasColumnName("default_currency_code")
            .HasColumnType("char(3)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'VND'")
            .ValueGeneratedNever()
            .HasComment("Tiền tệ hạch toán mặc định");
        builder.Property(x => x.PlanCode)
            .HasColumnName("plan_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(16)
            .HasComment("Gói dịch vụ (BASIC, PRO, ENTERPRISE)");
        builder.Property(x => x.MaxUsers)
            .HasColumnName("max_users")
            .HasColumnType("int")
            .HasColumnOrder(17)
            .HasComment("Số user tối đa theo gói; NULL = không giới hạn");
        builder.Property(x => x.MaxStorageMb)
            .HasColumnName("max_storage_mb")
            .HasColumnType("bigint")
            .HasColumnOrder(18)
            .HasComment("Dung lượng lưu trữ tối đa (MB)");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái tenant");
        builder.Property(x => x.TrialEndsAt)
            .HasColumnName("trial_ends_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Hết hạn dùng thử");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Hết hạn hợp đồng/thuê bao");
        builder.Property(x => x.IsolationMode)
            .HasColumnName("isolation_mode")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("'SHARED'")
            .ValueGeneratedNever()
            .HasComment("Mô hình cô lập dữ liệu ở các phân hệ: dùng chung DB, schema riêng, DB riêng");
        builder.Property(x => x.ConnectionInfo)
            .HasColumnName("connection_info")
            .HasColumnType("jsonb")
            .HasColumnOrder(23)
            .HasComment("Thông tin kết nối DB riêng (đã mã hóa) khi isolation_mode = DATABASE");
        builder.Property(x => x.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Thiết lập mở rộng (theme, màu thương hiệu...)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(25)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(26)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(27)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(28)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(29)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(30)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(31)
            .HasComment("Người xóa mềm (iam.users.id)");


        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("tenants_country_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DefaultCurrency)
            .WithMany()
            .HasForeignKey(x => x.DefaultCurrencyCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("tenants_default_currency_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.LogoFile)
            .WithMany()
            .HasForeignKey(x => x.LogoFileId)
            .HasConstraintName("fk_tenants_logo")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
