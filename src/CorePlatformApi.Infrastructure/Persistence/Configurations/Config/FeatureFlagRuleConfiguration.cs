// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho <c>config.feature_flag_rules</c>.
/// </summary>
public sealed class FeatureFlagRuleConfiguration : IEntityTypeConfiguration<FeatureFlagRule>
{
    public void Configure(EntityTypeBuilder<FeatureFlagRule> builder)
    {
        builder.ToTable("feature_flag_rules", "config", tb =>
        {
            tb.HasComment("Rule target của flag theo tenant/công ty/chi nhánh/vai trò/user, có thời gian hiệu lực");
            tb.HasCheckConstraint("feature_flag_rules_scope_type_check", "scope_type IN ('TENANT','COMPANY','BRANCH','ROLE','USER','PLAN','ENV')");
            tb.HasCheckConstraint("feature_flag_rules_percentage_check", "percentage BETWEEN 0 AND 100");
        });

        builder.HasKey(x => x.Id).HasName("feature_flag_rules_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.FlagKey)
            .HasColumnName("flag_key")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Flag");
        builder.Property(x => x.ScopeType)
            .HasColumnName("scope_type")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Đối tượng áp dụng");
        builder.Property(x => x.ScopeValue)
            .HasColumnName("scope_value")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Id hoặc mã đối tượng (uuid, mã gói, production/staging)");
        builder.Property(x => x.Enabled)
            .HasColumnName("enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bật/tắt cho đối tượng");
        builder.Property(x => x.Percentage)
            .HasColumnName("percentage")
            .HasColumnType("smallint")
            .HasColumnOrder(5)
            .HasComment("% rollout (PERCENTAGE)");
        builder.Property(x => x.Variant)
            .HasColumnName("variant")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(6)
            .HasComment("Biến thể trả về (VARIANT)");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Ưu tiên (số lớn xét trước)");
        builder.Property(x => x.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Bắt đầu hiệu lực");
        builder.Property(x => x.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Kết thúc hiệu lực");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.FlagKey, x.ScopeType, x.ScopeValue }, "uq_feature_flag_rules").IsUnique();

        builder.HasOne(x => x.Flag)
            .WithMany(p => p.FeatureFlagRules)
            .HasForeignKey(x => x.FlagKey)
            .HasConstraintName("feature_flag_rules_flag_key_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
