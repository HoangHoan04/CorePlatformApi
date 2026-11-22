// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Integration;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.api_keys</c>.
/// </summary>
public sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys", "integration", tb =>
        {
            tb.HasComment("API Key cấp cho đối tác/ứng dụng ngoài gọi vào ERP, có scope, IP whitelist, hạn dùng");
            tb.HasCheckConstraint("api_keys_environment_check", "environment IN ('LIVE','TEST')");
            tb.HasCheckConstraint("api_keys_status_check", "status IN ('ACTIVE','REVOKED','EXPIRED')");
        });

        builder.HasKey(x => x.Id).HasName("api_keys_pkey");

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
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.OauthClientId)
            .HasColumnName("oauth_client_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Gắn với client OAuth (nếu có)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên key (Kết nối website bán hàng)");
        builder.Property(x => x.KeyPrefix)
            .HasColumnName("key_prefix")
            .HasColumnType("varchar(16)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tiền tố hiển thị để nhận diện (berp_live_ab12)");
        builder.Property(x => x.KeyHash)
            .HasColumnName("key_hash")
            .HasColumnType("varchar(128)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("SHA-256 của API key (bản rõ chỉ hiển thị 1 lần khi tạo)");
        builder.Property(x => x.Scopes)
            .HasColumnName("scopes")
            .HasColumnType("text[]")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Scope được phép (iam.api_scopes.code)");
        builder.Property(x => x.OwnerUserId)
            .HasColumnName("owner_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người chịu trách nhiệm");
        builder.Property(x => x.ActAsUserId)
            .HasColumnName("act_as_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Thực thi dưới danh nghĩa user (để áp RBAC/data scope)");
        builder.Property(x => x.Environment)
            .HasColumnName("environment")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'LIVE'")
            .ValueGeneratedNever()
            .HasComment("Môi trường");
        builder.Property(x => x.IpWhitelist)
            .HasColumnName("ip_whitelist")
            .HasColumnType("cidr[]")
            .HasColumnOrder(10)
            .HasComment("IP được phép");
        builder.Property(x => x.AllowedOrigins)
            .HasColumnName("allowed_origins")
            .HasColumnType("text[]")
            .HasColumnOrder(11)
            .HasComment("Origin được phép (gọi từ trình duyệt)");
        builder.Property(x => x.RateLimitPolicyCode)
            .HasColumnName("rate_limit_policy_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(12)
            .HasComment("Chính sách rate limit");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Hết hạn");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Lần dùng gần nhất");
        builder.Property(x => x.LastUsedIp)
            .HasColumnName("last_used_ip")
            .HasColumnType("inet")
            .HasColumnOrder(15)
            .HasComment("IP dùng gần nhất");
        builder.Property(x => x.UsageCount)
            .HasColumnName("usage_count")
            .HasColumnType("bigint")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Tổng số lần gọi");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thu hồi lúc");
        builder.Property(x => x.RevokedBy)
            .HasColumnName("revoked_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người thu hồi");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.KeyPrefix, "api_keys_key_prefix_key").IsUnique();
        builder.HasIndex(x => x.KeyHash, "api_keys_key_hash_key").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("api_keys_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.OauthClient)
            .WithMany()
            .HasForeignKey(x => x.OauthClientId)
            .HasConstraintName("api_keys_oauth_client_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .HasConstraintName("api_keys_owner_user_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ActAsUser)
            .WithMany()
            .HasForeignKey(x => x.ActAsUserId)
            .HasConstraintName("api_keys_act_as_user_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
