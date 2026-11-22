// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Cấu hình EF Core cho <c>audit.login_logs</c>.
/// </summary>
public sealed class LoginLogConfiguration : IEntityTypeConfiguration<LoginLog>
{
    public void Configure(EntityTypeBuilder<LoginLog> builder)
    {
        builder.ToTable("login_logs", "audit", tb =>
        {
            tb.HasComment("Lịch sử đăng nhập/đăng xuất/refresh token (thành công & thất bại) kèm IP, thiết bị");
            tb.HasCheckConstraint("login_logs_event_type_check", "event_type IN ('LOGIN_SUCCESS','LOGIN_FAILED','LOGOUT','TOKEN_REFRESH','TOKEN_REUSE_DETECTED','MFA_SUCCESS','MFA_FAILED','ACCOUNT_LOCKED','SSO_LOGIN','IMPERSONATE')");
            tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql
        });

        builder.HasKey(x => new { x.Id, x.OccurredAt }).HasName("login_logs_pkey");

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
            .HasComment("Tenant");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("User (NULL nếu username không tồn tại)");
        builder.Property(x => x.UsernameAttempted)
            .HasColumnName("username_attempted")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .HasComment("Username/email đã nhập");
        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Sự kiện");
        builder.Property(x => x.FailureReason)
            .HasColumnName("failure_reason")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(5)
            .HasComment("Lý do thất bại (INVALID_PASSWORD, USER_NOT_FOUND, ACCOUNT_LOCKED, IP_NOT_ALLOWED, DEVICE_LIMIT)");
        builder.Property(x => x.AuthMethod)
            .HasColumnName("auth_method")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(6)
            .HasComment("Phương thức (PASSWORD, GOOGLE, OTP)");
        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(7)
            .HasComment("Ứng dụng");
        builder.Property(x => x.SessionId)
            .HasColumnName("session_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Phiên tạo ra");
        builder.Property(x => x.DeviceId)
            .HasColumnName("device_id")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Thiết bị");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(10)
            .HasComment("IP");
        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .HasColumnOrder(11)
            .HasComment("User-Agent");
        builder.Property(x => x.GeoCountry)
            .HasColumnName("geo_country")
            .HasColumnType("varchar(2)")
            .HasColumnOrder(12)
            .HasComment("Quốc gia (GeoIP)");
        builder.Property(x => x.GeoCity)
            .HasColumnName("geo_city")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(13)
            .HasComment("Thành phố (GeoIP)");
        builder.Property(x => x.IsSuspicious)
            .HasColumnName("is_suspicious")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Đánh dấu bất thường (IP lạ, quốc gia lạ)");
        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");

    }
}
