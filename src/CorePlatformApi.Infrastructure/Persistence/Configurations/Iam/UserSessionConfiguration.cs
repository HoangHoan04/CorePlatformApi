// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>user_sessions</c>.
/// </summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions", tb =>
        {
            tb.HasComment("Phiên đăng nhập SSO (sid). Một phiên trên một thiết bị/client; thu hồi phiên → thu hồi toàn bộ refresh token");
            tb.HasCheckConstraint("user_sessions_auth_method_check", "auth_method IN ('PASSWORD','PASSWORD_MFA','EXTERNAL','OTP','IMPERSONATION','API')");
            tb.HasCheckConstraint("user_sessions_revoked_reason_check", "revoked_reason IN ('LOGOUT','ADMIN_REVOKE','PASSWORD_CHANGED','ACCOUNT_LOCKED','DEVICE_LIMIT','TOKEN_REUSE','EXPIRED','ROLE_CHANGED')");
        });

        builder.HasKey(x => x.Id).HasName("user_sessions_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Session id (claim sid trong token)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.DeviceId)
            .HasColumnName("device_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Thiết bị");
        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("Ứng dụng khởi tạo phiên (iam.oauth_clients.client_id)");
        builder.Property(x => x.AuthMethod)
            .HasColumnName("auth_method")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'PASSWORD'")
            .ValueGeneratedNever()
            .HasComment("Phương thức xác thực");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Ngữ cảnh công ty đang chọn");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Ngữ cảnh chi nhánh đang chọn");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(8)
            .HasComment("IP khởi tạo");
        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("User-Agent");
        builder.Property(x => x.ImpersonatorId)
            .HasColumnName("impersonator_id")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Admin đang đăng nhập thay (hỗ trợ khách hàng)");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Bắt đầu");
        builder.Property(x => x.LastActivityAt)
            .HasColumnName("last_activity_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Hoạt động cuối (tính idle timeout)");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .IsRequired()
            .HasComment("Hết hạn tuyệt đối");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thu hồi lúc");
        builder.Property(x => x.RevokedReason)
            .HasColumnName("revoked_reason")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(15)
            .HasComment("Lý do thu hồi");
        builder.Property(x => x.RevokedBy)
            .HasColumnName("revoked_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người thu hồi");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.UserId, "ix_user_sessions_user_active")
            .HasFilter("revoked_at IS NULL");
        builder.HasIndex(x => x.ExpiresAt, "ix_user_sessions_expires");

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserSessions)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_sessions_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Device)
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .HasConstraintName("user_sessions_device_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
