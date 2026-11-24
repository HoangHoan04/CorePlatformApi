// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>model_alias_targets</c>.
/// </summary>
public sealed class AiModelAliasTargetConfiguration : IEntityTypeConfiguration<AiModelAliasTarget>
{
    public void Configure(EntityTypeBuilder<AiModelAliasTarget> builder)
    {
        builder.ToTable("model_alias_targets", tb =>
        {
            tb.HasComment("Danh sách model đích của alias theo thứ tự ưu tiên (model chính → fallback)");
        });

        builder.HasKey(x => x.Id).HasName("model_alias_targets_pkey");

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
        builder.Property(x => x.AliasId)
            .HasColumnName("alias_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Alias");
        builder.Property(x => x.ModelId)
            .HasColumnName("model_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Model");
        builder.Property(x => x.CredentialId)
            .HasColumnName("credential_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Ép dùng key này (NULL = tự chọn)");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("smallint")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("1 = chính, 2,3 = fallback khi lỗi/429");
        builder.Property(x => x.Weight)
            .HasColumnName("weight")
            .HasColumnType("smallint")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("100")
            .ValueGeneratedNever()
            .HasComment("Trọng số chia tải khi cùng priority");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.AliasId, x.ModelId }, "uq_ai_alias_targets").IsUnique();

        builder.HasOne(x => x.Alias)
            .WithMany(p => p.AiModelAliasTargets)
            .HasForeignKey(x => x.AliasId)
            .HasConstraintName("model_alias_targets_alias_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Model)
            .WithMany()
            .HasForeignKey(x => x.ModelId)
            .HasConstraintName("model_alias_targets_model_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Credential)
            .WithMany()
            .HasForeignKey(x => x.CredentialId)
            .HasConstraintName("model_alias_targets_credential_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
