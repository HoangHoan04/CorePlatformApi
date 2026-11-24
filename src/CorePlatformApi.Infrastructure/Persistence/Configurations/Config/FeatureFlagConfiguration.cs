// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho bảng <c>feature_flags</c>.
/// </summary>
public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("feature_flags", tb =>
        {
            tb.HasComment("Feature flag bật/tắt tính năng không cần deploy");
            tb.HasCheckConstraint("feature_flags_flag_type_check", "flag_type IN ('BOOLEAN','PERCENTAGE','VARIANT')");
        });

        builder.HasKey(x => x.Id).HasName("feature_flags_pkey");

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
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Khóa flag (wms.pos.offline_mode)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.FlagType)
            .HasColumnName("flag_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'BOOLEAN'")
            .ValueGeneratedNever()
            .HasComment("Bật/tắt, theo % rollout, theo biến thể");
        builder.Property(x => x.DefaultEnabled)
            .HasColumnName("default_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Mặc định khi không có rule khớp");
        builder.Property(x => x.Variants)
            .HasColumnName("variants")
            .HasColumnType("jsonb")
            .HasColumnOrder(8)
            .HasComment("Danh sách biến thể (VARIANT)");
        builder.Property(x => x.IsKillSwitch)
            .HasColumnName("is_kill_switch")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Công tắc khẩn cấp tắt tính năng toàn hệ thống");
        builder.Property(x => x.Owner)
            .HasColumnName("owner")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(10)
            .HasComment("Người/team phụ trách");
        builder.Property(x => x.RemoveAfter)
            .HasColumnName("remove_after")
            .HasColumnType("date")
            .HasColumnOrder(11)
            .HasComment("Hạn dọn flag khỏi code");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Flag còn được đánh giá");
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

        builder.HasAlternateKey(x => x.Key).HasName("feature_flags_key_key");

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("feature_flags_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
