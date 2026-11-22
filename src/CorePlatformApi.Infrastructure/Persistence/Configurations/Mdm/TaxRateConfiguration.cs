// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.tax_rates</c>.
/// </summary>
public sealed class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("tax_rates", "mdm", tb =>
        {
            tb.HasComment("Biểu thuế suất có hiệu lực theo thời gian (VD: VAT giảm 8% theo từng nghị định)");
            tb.HasCheckConstraint("tax_rates_rate_check", "rate >= 0");
            tb.HasCheckConstraint("tax_rates_calc_method_check", "calc_method IN ('PERCENT','NOT_SUBJECT','NOT_DECLARED','EXEMPT')");
            tb.HasCheckConstraint("ck_tax_rate_period", "effective_to IS NULL OR effective_to >= effective_from");
        });

        builder.HasKey(x => x.Id).HasName("tax_rates_pkey");

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
            .HasComment("Tenant (NULL = biểu thuế chuẩn)");
        builder.Property(x => x.TaxTypeCode)
            .HasColumnName("tax_type_code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại thuế");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Mã thuế suất (VAT10, VAT8, VAT5, VAT0, KCT, KKKNT)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên hiển thị (Thuế GTGT 10%)");
        builder.Property(x => x.Rate)
            .HasColumnName("rate")
            .HasColumnType("numeric(7,4)")
            .HasColumnOrder(5)
            .HasComment("Thuế suất % (NULL với KCT/KKKNT không tính thuế)");
        builder.Property(x => x.CalcMethod)
            .HasColumnName("calc_method")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'PERCENT'")
            .ValueGeneratedNever()
            .HasComment("Cách tính: theo %, không chịu thuế, không kê khai, miễn thuế");
        builder.Property(x => x.EinvoiceCode)
            .HasColumnName("einvoice_code")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .HasComment("Mã thuế suất trên hóa đơn điện tử (10%, 8%, KCT, KKKNT)");
        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("effective_from")
            .HasColumnType("date")
            .HasColumnOrder(8)
            .IsRequired()
            .HasComment("Hiệu lực từ");
        builder.Property(x => x.EffectiveTo)
            .HasColumnName("effective_to")
            .HasColumnType("date")
            .HasColumnOrder(9)
            .HasComment("Hiệu lực đến (NULL = còn hiệu lực)");
        builder.Property(x => x.LegalReference)
            .HasColumnName("legal_reference")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(10)
            .HasComment("Văn bản pháp lý");
        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Mặc định khi tạo chứng từ");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.Code, x.EffectiveFrom }, "uq_tax_rates")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("tax_rates_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.TaxType)
            .WithMany()
            .HasForeignKey(x => x.TaxTypeCode)
            .HasConstraintName("tax_rates_tax_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
