// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Config;

/// <summary>
/// Cấu hình EF Core cho <c>config.translation_keys</c>.
/// </summary>
public sealed class TranslationKeyConfiguration : IEntityTypeConfiguration<TranslationKey>
{
    public void Configure(EntityTypeBuilder<TranslationKey> builder)
    {
        builder.ToTable("translation_keys", "config", tb =>
        {
            tb.HasComment("Khóa dịch dùng chung (nhãn UI, thông báo lỗi API) theo namespace");
        });

        builder.HasKey(x => x.Id).HasName("translation_keys_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Namespace)
            .HasColumnName("namespace")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Nhóm: common, errors, validation, hrm, wms, crm");
        builder.Property(x => x.Key)
            .HasColumnName("key")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Khóa (button.save, AUTH_INVALID_TOKEN)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .HasComment("Ngữ cảnh cho người dịch");
        builder.Property(x => x.Platforms)
            .HasColumnName("platforms")
            .HasColumnType("text[]")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'{WEB,MOBILE,API}'")
            .ValueGeneratedNever()
            .HasComment("Nền tảng sử dụng (lọc khi export file JSON)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.Namespace, x.Key }, "uq_translation_keys").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("translation_keys_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
