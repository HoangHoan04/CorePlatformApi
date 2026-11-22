// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.modules</c>.
/// </summary>
public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("modules", "iam", tb =>
        {
            tb.HasComment("Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI...). Dùng cho phân quyền theo phân hệ và routing");
        });

        builder.HasKey(x => x.Id).HasName("modules_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã phân hệ (CORE, HRM, CRM, WMS, AI)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên phân hệ");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(3)
            .HasComment("Mô tả");
        builder.Property(x => x.BaseRoute)
            .HasColumnName("base_route")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("Tiền tố route trên gateway (/hrm)");
        builder.Property(x => x.Icon)
            .HasColumnName("icon")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(5)
            .HasComment("Icon menu");
        builder.Property(x => x.Color)
            .HasColumnName("color")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .HasComment("Màu nhận diện");
        builder.Property(x => x.WebUrl)
            .HasColumnName("web_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(7)
            .HasComment("URL ứng dụng web của phân hệ");
        builder.Property(x => x.IsCore)
            .HasColumnName("is_core")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Phân hệ lõi luôn bật cho mọi tenant");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
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

        builder.HasIndex(x => x.Code, "modules_code_key").IsUnique();
    }
}
