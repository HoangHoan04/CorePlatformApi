// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Cấu hình EF Core cho bảng <c>login_logs</c>.
/// </summary>
public sealed class LoginLogConfiguration : IEntityTypeConfiguration<LoginLog>
{
    public void Configure(EntityTypeBuilder<LoginLog> builder)
    {
        builder.ToTable("login_logs", tb =>
        {
            tb.HasComment("Lịch sử đăng nhập/đăng xuất/refresh token (thành công & thất bại) kèm IP, thiết bị");
            tb.HasCheckConstraint("login_logs_event_type_check", "event_type IN ('LOGIN_SUCCESS','LOGIN_FAILED','LOGOUT','TOKEN_REFRESH','TOKEN_REUSE_DETECTED','MFA_SUCCESS','MFA_FAILED','ACCOUNT_LOCKED','SSO_LOGIN','IMPERSONATE')");
        });

        builder.HasKey(x => x.Id).HasName("login_logs_pkey");

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
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.Id, x.OccurredAt }, "login_logs_id_occurred_at_key").IsUnique();
        builder.HasIndex(x => new { x.UserId, x.OccurredAt }, "ix_login_logs_user")
            .IsDescending(false, true);
        builder.HasIndex(x => new { x.TenantId, x.OccurredAt }, "ix_login_logs_tenant")
            .IsDescending(false, true);
        builder.HasIndex(x => new { x.IpAddress, x.OccurredAt }, "ix_login_logs_ip")
            .IsDescending(false, true);
    }
}
