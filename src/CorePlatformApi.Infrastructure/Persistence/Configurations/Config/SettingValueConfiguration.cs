// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho <c>config.setting_values</c>.
/// </summary>
public sealed class SettingValueConfiguration : IEntityTypeConfiguration<SettingValue>
{
    public void Configure(EntityTypeBuilder<SettingValue> builder)
    {
        builder.ToTable("setting_values", "config", tb =>
        {
            tb.HasComment("Giá trị cấu hình theo phạm vi. Thứ tự ưu tiên khi đọc: USER > BRANCH > COMPANY > TENANT > SYSTEM > default_value");
            tb.HasCheckConstraint("setting_values_scope_type_check", "scope_type IN ('SYSTEM','TENANT','COMPANY','BRANCH','USER')");
            tb.HasCheckConstraint("ck_setting_scope", "(scope_type = 'SYSTEM') = (scope_id IS NULL)");
        });

        builder.HasKey(x => x.Id).HasName("setting_values_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.SettingKey)
            .HasColumnName("setting_key")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Khóa cấu hình");
        builder.Property(x => x.ScopeType)
            .HasColumnName("scope_type")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phạm vi");
        builder.Property(x => x.ScopeId)
            .HasColumnName("scope_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Id tenant/công ty/chi nhánh/user (NULL khi SYSTEM)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Tenant (denormalize để cache theo tenant)");
        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .HasComment("Giá trị");
        builder.Property(x => x.ValueEncrypted)
            .HasColumnName("value_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Giá trị đã mã hóa (kiểu SECRET)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.SettingKey, x.ScopeType, x.ScopeId }, "uq_setting_values")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Setting)
            .WithMany(p => p.SettingValues)
            .HasForeignKey(x => x.SettingKey)
            .HasConstraintName("setting_values_setting_key_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
