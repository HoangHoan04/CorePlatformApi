// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho <c>gateway.security_header_policies</c>.
/// </summary>
public sealed class SecurityHeaderPolicyConfiguration : IEntityTypeConfiguration<SecurityHeaderPolicy>
{
    public void Configure(EntityTypeBuilder<SecurityHeaderPolicy> builder)
    {
        builder.ToTable("security_header_policies", "gateway", tb =>
        {
            tb.HasComment("Bộ security header gắn vào response (HSTS, CSP, X-Frame-Options...)");
        });

        builder.HasKey(x => x.Id).HasName("security_header_policies_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã chính sách (default, strict-admin)");
        builder.Property(x => x.Headers)
            .HasColumnName("headers")
            .HasColumnType("jsonb")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Map header → giá trị, VD: {\"Strict-Transport-Security\":\"max-age=31536000; includeSubDomains\"}");
        builder.Property(x => x.RemoveHeaders)
            .HasColumnName("remove_headers")
            .HasColumnType("text[]")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'{Server,X-Powered-By}'")
            .ValueGeneratedNever()
            .HasComment("Header cần gỡ khỏi response");
        builder.Property(x => x.ForceHttps)
            .HasColumnName("force_https")
            .HasColumnType("boolean")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Chuyển hướng HTTP → HTTPS");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.Code, "security_header_policies_code_key").IsUnique();
    }
}
