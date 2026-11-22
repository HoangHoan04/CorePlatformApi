// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho <c>gateway.cors_policies</c>.
/// </summary>
public sealed class CorsPolicyConfiguration : IEntityTypeConfiguration<CorsPolicy>
{
    public void Configure(EntityTypeBuilder<CorsPolicy> builder)
    {
        builder.ToTable("cors_policies", "gateway", tb =>
        {
            tb.HasComment("Chính sách CORS (toàn hệ thống hoặc theo tenant khi tenant có domain riêng)");
        });

        builder.HasKey(x => x.Id).HasName("cors_policies_pkey");

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
            .HasComment("Tenant (NULL = mặc định)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã chính sách");
        builder.Property(x => x.AllowedOrigins)
            .HasColumnName("allowed_origins")
            .HasColumnType("text[]")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Origin cho phép (https://admin.baseerp.vn); hỗ trợ wildcard subdomain");
        builder.Property(x => x.AllowedMethods)
            .HasColumnName("allowed_methods")
            .HasColumnType("text[]")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'{GET,POST,PUT,PATCH,DELETE,OPTIONS}'")
            .ValueGeneratedNever()
            .HasComment("Method cho phép");
        builder.Property(x => x.AllowedHeaders)
            .HasColumnName("allowed_headers")
            .HasColumnType("text[]")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'{Authorization,Content-Type,X-Tenant-Id,X-Request-Id,Accept-Language}'")
            .ValueGeneratedNever()
            .HasComment("Header cho phép");
        builder.Property(x => x.ExposedHeaders)
            .HasColumnName("exposed_headers")
            .HasColumnType("text[]")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'{X-Trace-Id,X-RateLimit-Remaining,Content-Disposition}'")
            .ValueGeneratedNever()
            .HasComment("Header trình duyệt được đọc");
        builder.Property(x => x.AllowCredentials)
            .HasColumnName("allow_credentials")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Cho phép cookie/credentials");
        builder.Property(x => x.MaxAgeSeconds)
            .HasColumnName("max_age_seconds")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("600")
            .ValueGeneratedNever()
            .HasComment("Cache preflight");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.Code }, "uq_cors_policies")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("cors_policies_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
