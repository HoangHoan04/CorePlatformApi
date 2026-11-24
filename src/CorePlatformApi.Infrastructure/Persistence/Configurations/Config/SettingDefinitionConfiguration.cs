// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho bảng <c>setting_definitions</c>.
/// </summary>
public sealed class SettingDefinitionConfiguration : IEntityTypeConfiguration<SettingDefinition>
{
    public void Configure(EntityTypeBuilder<SettingDefinition> builder)
    {
        builder.ToTable("setting_definitions", tb =>
        {
            tb.HasComment("Định nghĩa khóa cấu hình (kiểu dữ liệu, mặc định, phạm vi cho phép ghi đè) — thay cho hardcode");
            tb.HasCheckConstraint("setting_definitions_data_type_check", "data_type IN ('STRING','INT','DECIMAL','BOOL','DATE','TIME','JSON','ENUM','SECRET')");
        });

        builder.HasKey(x => x.Id).HasName("setting_definitions_pkey");

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
        builder.Property(x => x.Key)
            .HasColumnName("key")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Khóa cấu hình (hrm.attendance.late_grace_minutes)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ");
        builder.Property(x => x.GroupName)
            .HasColumnName("group_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("Nhóm hiển thị trên màn hình cài đặt");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên hiển thị");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Giải thích");
        builder.Property(x => x.DataType)
            .HasColumnName("data_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Kiểu dữ liệu");
        builder.Property(x => x.DefaultValue)
            .HasColumnName("default_value")
            .HasColumnType("jsonb")
            .HasColumnOrder(8)
            .HasComment("Giá trị mặc định");
        builder.Property(x => x.Options)
            .HasColumnName("options")
            .HasColumnType("jsonb")
            .HasColumnOrder(9)
            .HasComment("Danh sách lựa chọn (ENUM)");
        builder.Property(x => x.Validation)
            .HasColumnName("validation")
            .HasColumnType("jsonb")
            .HasColumnOrder(10)
            .HasComment("Ràng buộc (min, max, regex)");
        builder.Property(x => x.AllowedScopes)
            .HasColumnName("allowed_scopes")
            .HasColumnType("text[]")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'{SYSTEM,TENANT}'")
            .ValueGeneratedNever()
            .HasComment("Phạm vi được ghi đè: SYSTEM, TENANT, COMPANY, BRANCH, USER");
        builder.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Trả về cho frontend (không bí mật)");
        builder.Property(x => x.RequiresRestart)
            .HasColumnName("requires_restart")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cần restart service để có hiệu lực");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasAlternateKey(x => x.Key).HasName("setting_definitions_key_key");

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("setting_definitions_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
