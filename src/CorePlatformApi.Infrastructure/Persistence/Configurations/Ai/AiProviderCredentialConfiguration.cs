// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.provider_credentials</c>.
/// </summary>
public sealed class AiProviderCredentialConfiguration : IEntityTypeConfiguration<AiProviderCredential>
{
    public void Configure(EntityTypeBuilder<AiProviderCredential> builder)
    {
        builder.ToTable("provider_credentials", "ai", tb =>
        {
            tb.HasComment("API key của nhà cung cấp AI — lưu tập trung, các service con không giữ key. Tenant có thể dùng key riêng (BYOK)");
            tb.HasCheckConstraint("provider_credentials_status_check", "status IN ('ACTIVE','DISABLED','EXHAUSTED','INVALID')");
        });

        builder.HasKey(x => x.Id).HasName("provider_credentials_pkey");

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
            .HasComment("Tenant (NULL = key hệ thống)");
        builder.Property(x => x.ProviderId)
            .HasColumnName("provider_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Nhà cung cấp");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên key");
        builder.Property(x => x.ApiKeyEncrypted)
            .HasColumnName("api_key_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("API key đã mã hóa");
        builder.Property(x => x.BaseUrlOverride)
            .HasColumnName("base_url_override")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .HasComment("URL riêng (Azure endpoint, proxy)");
        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(6)
            .HasComment("Org/Project id phía provider");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự dùng khi có nhiều key (cân bằng tải/failover)");
        builder.Property(x => x.MonthlyBudgetUsd)
            .HasColumnName("monthly_budget_usd")
            .HasColumnType("numeric(12,2)")
            .HasColumnOrder(8)
            .HasComment("Ngân sách tháng cho key");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Dùng gần nhất");
        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text")
            .HasColumnOrder(11)
            .HasComment("Lỗi gần nhất (401, 429...)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");


        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("provider_credentials_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .HasConstraintName("provider_credentials_provider_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
