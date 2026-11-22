// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Cấu hình EF Core cho <c>audit.security_events</c>.
/// </summary>
public sealed class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.ToTable("security_events", "audit", tb =>
        {
            tb.HasComment("Sự kiện bảo mật quan trọng: đổi/reset mật khẩu, khóa/mở khóa, thay đổi vai trò/quyền, cấp API key, bật/tắt 2FA");
            tb.HasCheckConstraint("security_events_severity_check", "severity IN ('INFO','WARNING','CRITICAL')");
            tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql
        });

        builder.HasKey(x => new { x.Id, x.OccurredAt }).HasName("security_events_pkey");

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
        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasColumnType("varchar(40)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại (PASSWORD_CHANGED, PASSWORD_RESET, USER_LOCKED, USER_UNLOCKED, ROLE_ASSIGNED, ROLE_REVOKED, ROLE_PERMISSION_CHANGED, MFA_ENABLED, API_KEY_CREATED, SESSION_REVOKED...)");
        builder.Property(x => x.Severity)
            .HasColumnName("severity")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'INFO'")
            .ValueGeneratedNever()
            .HasComment("Mức độ");
        builder.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Người thực hiện (NULL = hệ thống)");
        builder.Property(x => x.TargetUserId)
            .HasColumnName("target_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("User bị tác động");
        builder.Property(x => x.TargetType)
            .HasColumnName("target_type")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(6)
            .HasComment("Loại đối tượng bị tác động (ROLE, API_KEY, OAUTH_CLIENT)");
        builder.Property(x => x.TargetId)
            .HasColumnName("target_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Id đối tượng");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(8)
            .HasComment("Mô tả dễ đọc");
        builder.Property(x => x.Details)
            .HasColumnName("details")
            .HasColumnType("jsonb")
            .HasColumnOrder(9)
            .HasComment("Chi tiết (quyền thêm/bớt, vai trò cũ/mới)");
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
        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(12)
            .HasComment("Trace id");
        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");

    }
}
