// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho <c>config.setting_definitions</c>.
/// </summary>
public sealed class SettingDefinitionConfiguration : IEntityTypeConfiguration<SettingDefinition>
{
    public void Configure(EntityTypeBuilder<SettingDefinition> builder)
    {
        builder.ToTable("setting_definitions", "config", tb =>
        {
            tb.HasComment("Định nghĩa khóa cấu hình (kiểu dữ liệu, mặc định, phạm vi cho phép ghi đè) — thay cho hardcode");
            tb.HasCheckConstraint("setting_definitions_data_type_check", "data_type IN ('STRING','INT','DECIMAL','BOOL','DATE','TIME','JSON','ENUM','SECRET')");
        });

        builder.HasKey(x => x.Key).HasName("setting_definitions_pkey");

        builder.Property(x => x.Key)
            .HasColumnName("key")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Khóa cấu hình (hrm.attendance.late_grace_minutes)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Phân hệ");
        builder.Property(x => x.GroupName)
            .HasColumnName("group_name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .HasComment("Nhóm hiển thị trên màn hình cài đặt");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên hiển thị");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .HasComment("Giải thích");
        builder.Property(x => x.DataType)
            .HasColumnName("data_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Kiểu dữ liệu");
        builder.Property(x => x.DefaultValue)
            .HasColumnName("default_value")
            .HasColumnType("jsonb")
            .HasColumnOrder(6)
            .HasComment("Giá trị mặc định");
        builder.Property(x => x.Options)
            .HasColumnName("options")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .HasComment("Danh sách lựa chọn (ENUM)");
        builder.Property(x => x.Validation)
            .HasColumnName("validation")
            .HasColumnType("jsonb")
            .HasColumnOrder(8)
            .HasComment("Ràng buộc (min, max, regex)");
        builder.Property(x => x.AllowedScopes)
            .HasColumnName("allowed_scopes")
            .HasColumnType("text[]")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'{SYSTEM,TENANT}'")
            .ValueGeneratedNever()
            .HasComment("Phạm vi được ghi đè: SYSTEM, TENANT, COMPANY, BRANCH, USER");
        builder.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Trả về cho frontend (không bí mật)");
        builder.Property(x => x.RequiresRestart)
            .HasColumnName("requires_restart")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cần restart service để có hiệu lực");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
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


        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("setting_definitions_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
