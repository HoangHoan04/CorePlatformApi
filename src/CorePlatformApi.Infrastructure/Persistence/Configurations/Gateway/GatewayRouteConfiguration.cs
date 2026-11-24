// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho bảng <c>routes</c>.
/// </summary>
public sealed class GatewayRouteConfiguration : IEntityTypeConfiguration<GatewayRoute>
{
    public void Configure(EntityTypeBuilder<GatewayRoute> builder)
    {
        builder.ToTable("routes", tb =>
        {
            tb.HasComment("Route định tuyến: /auth/*, /hrm/*, /wms/*, /crm/*, /ai/* → cluster nội bộ");
            tb.HasCheckConstraint("routes_auth_mode_check", "auth_mode IN ('ANONYMOUS','JWT','INTROSPECT','API_KEY','JWT_OR_API_KEY','MTLS')");
        });

        builder.HasKey(x => x.Id).HasName("routes_pkey");

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
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã route");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Phân hệ");
        builder.Property(x => x.ClusterId)
            .HasColumnName("cluster_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Cluster đích");
        builder.Property(x => x.PathPattern)
            .HasColumnName("path_pattern")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Mẫu path (/hrm/{**catch-all})");
        builder.Property(x => x.Methods)
            .HasColumnName("methods")
            .HasColumnType("text[]")
            .HasColumnOrder(6)
            .HasComment("Method khớp (NULL = tất cả)");
        builder.Property(x => x.Hosts)
            .HasColumnName("hosts")
            .HasColumnType("text[]")
            .HasColumnOrder(7)
            .HasComment("Host khớp (NULL = tất cả)");
        builder.Property(x => x.PathTransform)
            .HasColumnName("path_transform")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Biến đổi path (bỏ tiền tố /hrm → /api)");
        builder.Property(x => x.OrderNo)
            .HasColumnName("order_no")
            .HasColumnType("int")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự khớp (nhỏ ưu tiên)");
        builder.Property(x => x.AuthMode)
            .HasColumnName("auth_mode")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'JWT'")
            .ValueGeneratedNever()
            .HasComment("Cách xác thực tại gateway: verify JWT cục bộ hoặc introspect với IdP");
        builder.Property(x => x.RequiredScopes)
            .HasColumnName("required_scopes")
            .HasColumnType("text[]")
            .HasColumnOrder(11)
            .HasComment("Scope bắt buộc");
        builder.Property(x => x.RequiredPermission)
            .HasColumnName("required_permission")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(12)
            .HasComment("Quyền bắt buộc kiểm tra sớm tại gateway (tùy chọn)");
        builder.Property(x => x.RateLimitPolicyId)
            .HasColumnName("rate_limit_policy_id")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Rate limit");
        builder.Property(x => x.CorsPolicyId)
            .HasColumnName("cors_policy_id")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("CORS");
        builder.Property(x => x.SecurityHeaderPolicyId)
            .HasColumnName("security_header_policy_id")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Security header");
        builder.Property(x => x.TimeoutMs)
            .HasColumnName("timeout_ms")
            .HasColumnType("int")
            .HasColumnOrder(16)
            .HasComment("Timeout riêng (ghi đè cluster)");
        builder.Property(x => x.MaxRequestBodyMb)
            .HasColumnName("max_request_body_mb")
            .HasColumnType("int")
            .HasColumnOrder(17)
            .HasComment("Giới hạn kích thước body (upload)");
        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .HasColumnType("smallint")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Retry khi upstream lỗi (chỉ method idempotent)");
        builder.Property(x => x.CacheSeconds)
            .HasColumnName("cache_seconds")
            .HasColumnType("int")
            .HasColumnOrder(19)
            .HasComment("Cache response tại gateway (danh mục MDM)");
        builder.Property(x => x.ForwardHeaders)
            .HasColumnName("forward_headers")
            .HasColumnType("jsonb")
            .HasColumnOrder(20)
            .HasComment("Header thêm khi chuyển tiếp (X-User-Id, X-Tenant-Id, X-Permissions)");
        builder.Property(x => x.WrapResponse)
            .HasColumnName("wrap_response")
            .HasColumnType("boolean")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bọc response/lỗi theo định dạng chuẩn");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang bật");
        builder.Property(x => x.MaintenanceMessage)
            .HasColumnName("maintenance_message")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(23)
            .HasComment("Thông báo bảo trì (khi có giá trị gateway trả 503)");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(25)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(26)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(27)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(28)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(29)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(30)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Code, "routes_code_key").IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("routes_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Cluster)
            .WithMany()
            .HasForeignKey(x => x.ClusterId)
            .HasConstraintName("routes_cluster_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.RateLimitPolicy)
            .WithMany()
            .HasForeignKey(x => x.RateLimitPolicyId)
            .HasConstraintName("routes_rate_limit_policy_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.CorsPolicy)
            .WithMany()
            .HasForeignKey(x => x.CorsPolicyId)
            .HasConstraintName("routes_cors_policy_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SecurityHeaderPolicy)
            .WithMany()
            .HasForeignKey(x => x.SecurityHeaderPolicyId)
            .HasConstraintName("routes_security_header_policy_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
