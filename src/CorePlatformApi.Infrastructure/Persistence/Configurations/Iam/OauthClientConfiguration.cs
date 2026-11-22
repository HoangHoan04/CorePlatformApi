// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.oauth_clients</c>.
/// </summary>
public sealed class OauthClientConfiguration : IEntityTypeConfiguration<OauthClient>
{
    public void Configure(EntityTypeBuilder<OauthClient> builder)
    {
        builder.ToTable("oauth_clients", "iam", tb =>
        {
            tb.HasComment("Ứng dụng client (OAuth2/OIDC): Core Admin, HRM Web, HRM Mobile, WMS Mobile, service nội bộ, ứng dụng đối tác");
            tb.HasCheckConstraint("oauth_clients_client_type_check", "client_type IN ('PUBLIC','CONFIDENTIAL')");
            tb.HasCheckConstraint("oauth_clients_app_type_check", "app_type IN ('WEB','SPA','MOBILE','SERVICE','PARTNER')");
            tb.HasCheckConstraint("oauth_clients_status_check", "status IN ('ACTIVE','DISABLED','REVOKED')");
        });

        builder.HasKey(x => x.Id).HasName("oauth_clients_pkey");

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
            .HasComment("Tenant sở hữu (NULL = client hệ thống dùng chung)");
        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("client_id công khai");
        builder.Property(x => x.ClientSecretHash)
            .HasColumnName("client_secret_hash")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .HasComment("Hash client_secret (NULL với public client như SPA/Mobile dùng PKCE)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên ứng dụng");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Phân hệ gắn với client (HRM Mobile → HRM)");
        builder.Property(x => x.ClientType)
            .HasColumnName("client_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Public (SPA, mobile) hay Confidential (backend)");
        builder.Property(x => x.AppType)
            .HasColumnName("app_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasComment("Loại ứng dụng");
        builder.Property(x => x.GrantTypes)
            .HasColumnName("grant_types")
            .HasColumnType("text[]")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'{authorization_code,refresh_token}'")
            .ValueGeneratedNever()
            .HasComment("Grant cho phép: authorization_code, refresh_token, client_credentials, password");
        builder.Property(x => x.RedirectUris)
            .HasColumnName("redirect_uris")
            .HasColumnType("text[]")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Danh sách redirect URI hợp lệ");
        builder.Property(x => x.PostLogoutRedirectUris)
            .HasColumnName("post_logout_redirect_uris")
            .HasColumnType("text[]")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("URI chuyển hướng sau đăng xuất");
        builder.Property(x => x.AllowedOrigins)
            .HasColumnName("allowed_origins")
            .HasColumnType("text[]")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Origin CORS cho client");
        builder.Property(x => x.AllowedScopes)
            .HasColumnName("allowed_scopes")
            .HasColumnType("text[]")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("'{openid,profile,offline_access}'")
            .ValueGeneratedNever()
            .HasComment("Scope cho phép (iam.api_scopes.code)");
        builder.Property(x => x.RequirePkce)
            .HasColumnName("require_pkce")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bắt buộc PKCE");
        builder.Property(x => x.RequireConsent)
            .HasColumnName("require_consent")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Hiển thị màn hình đồng ý (ứng dụng bên thứ 3)");
        builder.Property(x => x.AccessTokenMinutes)
            .HasColumnName("access_token_minutes")
            .HasColumnType("int")
            .HasColumnOrder(16)
            .HasComment("Ghi đè thời hạn access token");
        builder.Property(x => x.RefreshTokenDays)
            .HasColumnName("refresh_token_days")
            .HasColumnType("int")
            .HasColumnOrder(17)
            .HasComment("Ghi đè thời hạn refresh token");
        builder.Property(x => x.RotateRefreshToken)
            .HasColumnName("rotate_refresh_token")
            .HasColumnType("boolean")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bật Refresh Token Rotation");
        builder.Property(x => x.IpWhitelist)
            .HasColumnName("ip_whitelist")
            .HasColumnType("cidr[]")
            .HasColumnOrder(19)
            .HasComment("IP được phép dùng client_credentials");
        builder.Property(x => x.RateLimitPolicyCode)
            .HasColumnName("rate_limit_policy_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(20)
            .HasComment("Chính sách rate limit áp dụng (gateway.rate_limit_policies.code)");
        builder.Property(x => x.LogoUrl)
            .HasColumnName("logo_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(21)
            .HasComment("Logo hiển thị trên trang đồng ý");
        builder.Property(x => x.SecretExpiresAt)
            .HasColumnName("secret_expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Hạn của client_secret");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Lần dùng gần nhất");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(25)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(26)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(27)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(28)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(29)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(30)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(31)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => x.ClientId, "oauth_clients_client_id_key").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("oauth_clients_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("oauth_clients_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
