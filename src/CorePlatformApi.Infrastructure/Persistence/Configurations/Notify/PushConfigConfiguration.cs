// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho bảng <c>push_configs</c>.
/// </summary>
public sealed class PushConfigConfiguration : IEntityTypeConfiguration<PushConfig>
{
    public void Configure(EntityTypeBuilder<PushConfig> builder)
    {
        builder.ToTable("push_configs", tb =>
        {
            tb.HasComment("Cấu hình nhà cung cấp push theo ứng dụng mobile (FCM, APNs)");
            tb.HasCheckConstraint("push_configs_provider_check", "provider IN ('FCM','APNS','HMS')");
        });

        builder.HasKey(x => x.Id).HasName("push_configs_pkey");

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
            .HasComment("Tenant (NULL = app hệ thống; khác NULL khi build app white-label)");
        builder.Property(x => x.AppCode)
            .HasColumnName("app_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã app (HRM_MOBILE, WMS_MOBILE)");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Nhà cung cấp");
        builder.Property(x => x.ProjectId)
            .HasColumnName("project_id")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .HasComment("Firebase project id");
        builder.Property(x => x.BundleId)
            .HasColumnName("bundle_id")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .HasComment("iOS bundle id / Android package name");
        builder.Property(x => x.CredentialsEncrypted)
            .HasColumnName("credentials_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Service account JSON (FCM) hoặc .p8 key (APNs) đã mã hóa");
        builder.Property(x => x.ApnsKeyId)
            .HasColumnName("apns_key_id")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(7)
            .HasComment("APNs Key ID");
        builder.Property(x => x.ApnsTeamId)
            .HasColumnName("apns_team_id")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(8)
            .HasComment("Apple Team ID");
        builder.Property(x => x.IsProduction)
            .HasColumnName("is_production")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Môi trường production/sandbox");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.TenantId, x.AppCode, x.Provider }, "uq_push_configs")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("push_configs_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
