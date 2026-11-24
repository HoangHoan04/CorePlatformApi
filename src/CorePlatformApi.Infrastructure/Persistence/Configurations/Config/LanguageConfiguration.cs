// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho bảng <c>languages</c>.
/// </summary>
public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("languages", tb =>
        {
            tb.HasComment("Ngôn ngữ hệ thống hỗ trợ");
            tb.HasCheckConstraint("languages_direction_check", "direction IN ('LTR','RTL')");
        });

        builder.HasKey(x => x.Id).HasName("languages_pkey");

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
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã BCP-47 (vi-VN, en-US)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên tiếng Việt");
        builder.Property(x => x.NativeName)
            .HasColumnName("native_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên bản ngữ (Tiếng Việt, English)");
        builder.Property(x => x.Direction)
            .HasColumnName("direction")
            .HasColumnType("varchar(3)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'LTR'")
            .ValueGeneratedNever()
            .HasComment("Hướng chữ");
        builder.Property(x => x.DateFormat)
            .HasColumnName("date_format")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'dd/MM/yyyy'")
            .ValueGeneratedNever()
            .HasComment("Định dạng ngày");
        builder.Property(x => x.NumberFormat)
            .HasColumnName("number_format")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .HasComment("Dấu thập phân, nhóm nghìn");
        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Ngôn ngữ mặc định");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang hỗ trợ");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự");
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

        builder.HasAlternateKey(x => x.Code).HasName("languages_code_key");
    }
}
