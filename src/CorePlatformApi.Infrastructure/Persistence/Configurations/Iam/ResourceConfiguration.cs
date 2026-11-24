// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>resources</c>.
/// </summary>
public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources", tb =>
        {
            tb.HasComment("Tài nguyên/chức năng (menu, màn hình, API) của từng phân hệ dạng cây. Permission = resource + action");
            tb.HasCheckConstraint("resources_resource_type_check", "resource_type IN ('MENU_GROUP','FUNCTION','REPORT','API')");
        });

        builder.HasKey(x => x.Id).HasName("resources_pkey");

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
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phân hệ");
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Tài nguyên cha (nhóm menu)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Mã tài nguyên (EMPLOYEE, LEAVE_REQUEST, GOODS_RECEIPT)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên hiển thị");
        builder.Property(x => x.ResourceType)
            .HasColumnName("resource_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'FUNCTION'")
            .ValueGeneratedNever()
            .HasComment("Loại: nhóm menu, chức năng, báo cáo, API thuần");
        builder.Property(x => x.Route)
            .HasColumnName("route")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(7)
            .HasComment("Route frontend (/hrm/employees)");
        builder.Property(x => x.Icon)
            .HasColumnName("icon")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(8)
            .HasComment("Icon menu");
        builder.Property(x => x.SupportsDataScope)
            .HasColumnName("supports_data_scope")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Tài nguyên có áp dụng phạm vi dữ liệu không");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự menu");
        builder.Property(x => x.IsVisible)
            .HasColumnName("is_visible")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Hiển thị trên menu");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn sử dụng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.ModuleId, x.Code }, "uq_resources_code").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("resources_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("resources_parent_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
