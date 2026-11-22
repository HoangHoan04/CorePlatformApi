// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho <c>config.translations</c>.
/// </summary>
public sealed class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable("translations", "config", tb =>
        {
            tb.HasComment("Bản dịch theo ngôn ngữ; tenant có thể ghi đè thuật ngữ riêng (VD: \"Nhân viên\" → \"Cộng sự\")");
        });

        builder.HasKey(x => x.Id).HasName("translations_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.KeyId)
            .HasColumnName("key_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Khóa dịch");
        builder.Property(x => x.LanguageCode)
            .HasColumnName("language_code")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Ngôn ngữ");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Tenant ghi đè (NULL = bản dịch chuẩn)");
        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Nội dung dịch (hỗ trợ ICU {count, plural, ...})");
        builder.Property(x => x.IsReviewed)
            .HasColumnName("is_reviewed")
            .HasColumnType("boolean")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Đã duyệt");
        builder.Property(x => x.IsMachine)
            .HasColumnName("is_machine")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Dịch máy (AI)");
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

        builder.HasIndex(x => new { x.KeyId, x.LanguageCode, x.TenantId }, "uq_translations")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Key)
            .WithMany(p => p.Translations)
            .HasForeignKey(x => x.KeyId)
            .HasConstraintName("translations_key_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageCode)
            .HasConstraintName("translations_language_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("translations_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
