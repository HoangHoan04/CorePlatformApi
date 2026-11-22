// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.users</c>.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "iam", tb =>
        {
            tb.HasComment("Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web & Mobile)");
            tb.HasCheckConstraint("users_gender_check", "gender IN ('MALE','FEMALE','OTHER')");
            tb.HasCheckConstraint("users_user_type_check", "user_type IN ('INTERNAL','PARTNER','CUSTOMER','SERVICE')");
            tb.HasCheckConstraint("users_status_check", "status IN ('PENDING','ACTIVE','LOCKED','DISABLED')");
            tb.HasCheckConstraint("users_mfa_type_check", "mfa_type IN ('TOTP','EMAIL_OTP','SMS_OTP')");
        });

        builder.HasKey(x => x.Id).HasName("users_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (sub trong JWT)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant (NULL = Platform Admin quản trị toàn nền tảng)");
        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasColumnType("citext")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên đăng nhập (duy nhất trong tenant)");
        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("citext")
            .HasColumnOrder(3)
            .HasComment("Email (đăng nhập, khôi phục mật khẩu)");
        builder.Property(x => x.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(4)
            .HasComment("Thời điểm xác thực email");
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .HasComment("Số điện thoại (định dạng E.164 khuyến nghị)");
        builder.Property(x => x.PhoneVerifiedAt)
            .HasColumnName("phone_verified_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Thời điểm xác thực SĐT");
        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(7)
            .HasComment("Hash mật khẩu (Argon2id/BCrypt/PBKDF2). NULL nếu chỉ đăng nhập SSO ngoài");
        builder.Property(x => x.PasswordChangedAt)
            .HasColumnName("password_changed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Lần đổi mật khẩu gần nhất (tính hết hạn mật khẩu)");
        builder.Property(x => x.MustChangePassword)
            .HasColumnName("must_change_password")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Ép đổi mật khẩu ở lần đăng nhập kế tiếp (lần đầu / sau reset)");
        builder.Property(x => x.SecurityStamp)
            .HasColumnName("security_stamp")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("encode(gen_random_bytes(16), 'hex')")
            .ValueGeneratedNever()
            .HasComment("Đổi giá trị khi đổi mật khẩu/quyền/khóa → vô hiệu toàn bộ token cũ");
        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasComment("Họ tên đầy đủ");
        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(12)
            .HasComment("Tên hiển thị");
        builder.Property(x => x.AvatarFileId)
            .HasColumnName("avatar_file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Ảnh đại diện (storage.files)");
        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(14)
            .HasComment("Giới tính (phục vụ xưng hô trong thông báo)");
        builder.Property(x => x.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date")
            .HasColumnOrder(15)
            .HasComment("Ngày sinh");
        builder.Property(x => x.EmployeeId)
            .HasColumnName("employee_id")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Liên kết hồ sơ nhân viên bên HRM (không FK vì khác service)");
        builder.Property(x => x.EmployeeCode)
            .HasColumnName("employee_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(17)
            .HasComment("Mã nhân viên (denormalize để hiển thị)");
        builder.Property(x => x.DefaultCompanyId)
            .HasColumnName("default_company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Công ty mặc định khi đăng nhập");
        builder.Property(x => x.DefaultBranchId)
            .HasColumnName("default_branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Chi nhánh mặc định");
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Phòng ban chính (OWN/DEPARTMENT scope)");
        builder.Property(x => x.DirectManagerId)
            .HasColumnName("direct_manager_id")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Quản lý trực tiếp (workflow approver DIRECT_MANAGER)");
        builder.Property(x => x.JobTitle)
            .HasColumnName("job_title")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(22)
            .HasComment("Chức danh hiển thị");
        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(23)
            .HasComment("Ngôn ngữ ưu tiên (NULL = theo tenant)");
        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(24)
            .HasComment("Múi giờ ưu tiên");
        builder.Property(x => x.UserType)
            .HasColumnName("user_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(25)
            .IsRequired()
            .HasDefaultValueSql("'INTERNAL'")
            .ValueGeneratedNever()
            .HasComment("Loại: nhân viên, đối tác, khách hàng portal, tài khoản dịch vụ");
        builder.Property(x => x.IsTenantAdmin)
            .HasColumnName("is_tenant_admin")
            .HasColumnType("boolean")
            .HasColumnOrder(26)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Quản trị viên tenant (bỏ qua RBAC trong tenant)");
        builder.Property(x => x.IsPlatformAdmin)
            .HasColumnName("is_platform_admin")
            .HasColumnType("boolean")
            .HasColumnOrder(27)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Quản trị viên nền tảng (đa tenant)");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(28)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái: chờ kích hoạt, hoạt động, bị khóa, vô hiệu");
        builder.Property(x => x.LockedReason)
            .HasColumnName("locked_reason")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(29)
            .HasComment("Lý do khóa");
        builder.Property(x => x.LockedAt)
            .HasColumnName("locked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(30)
            .HasComment("Thời điểm khóa");
        builder.Property(x => x.LockedUntil)
            .HasColumnName("locked_until")
            .HasColumnType("timestamptz")
            .HasColumnOrder(31)
            .HasComment("Khóa tạm đến thời điểm (do nhập sai mật khẩu); NULL = khóa vô thời hạn");
        builder.Property(x => x.LockedBy)
            .HasColumnName("locked_by")
            .HasColumnType("uuid")
            .HasColumnOrder(32)
            .HasComment("Người khóa (NULL = hệ thống tự khóa)");
        builder.Property(x => x.FailedLoginCount)
            .HasColumnName("failed_login_count")
            .HasColumnType("int")
            .HasColumnOrder(33)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần đăng nhập sai liên tiếp");
        builder.Property(x => x.LastFailedLoginAt)
            .HasColumnName("last_failed_login_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(34)
            .HasComment("Lần đăng nhập sai gần nhất");
        builder.Property(x => x.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(35)
            .HasComment("Lần đăng nhập thành công gần nhất");
        builder.Property(x => x.LastLoginIp)
            .HasColumnName("last_login_ip")
            .HasColumnType("inet")
            .HasColumnOrder(36)
            .HasComment("IP đăng nhập gần nhất");
        builder.Property(x => x.MfaEnabled)
            .HasColumnName("mfa_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(37)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bật xác thực 2 lớp");
        builder.Property(x => x.MfaType)
            .HasColumnName("mfa_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(38)
            .HasComment("Phương thức 2FA");
        builder.Property(x => x.MfaSecretEncrypted)
            .HasColumnName("mfa_secret_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(39)
            .HasComment("Secret TOTP đã mã hóa");
        builder.Property(x => x.MaxDevices)
            .HasColumnName("max_devices")
            .HasColumnType("int")
            .HasColumnOrder(40)
            .HasComment("Số thiết bị đăng nhập đồng thời tối đa (NULL = theo chính sách tenant)");
        builder.Property(x => x.ActivatedAt)
            .HasColumnName("activated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(41)
            .HasComment("Thời điểm kích hoạt tài khoản");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(42)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(43)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(44)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(45)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(46)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(47)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(48)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.Username }, "uq_users_username")
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("NOT is_deleted");
        builder.HasIndex(x => new { x.TenantId, x.Email }, "uq_users_email")
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("email IS NOT NULL AND NOT is_deleted");
        builder.HasIndex(x => new { x.TenantId, x.Phone }, "ix_users_phone")
            .HasFilter("NOT is_deleted");
        builder.HasIndex(x => x.DepartmentId, "ix_users_department")
            .HasFilter("NOT is_deleted");
        builder.HasIndex(x => x.EmployeeId, "ix_users_employee");
        builder.HasIndex(x => x.FullName, "ix_users_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("users_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DefaultCompany)
            .WithMany()
            .HasForeignKey(x => x.DefaultCompanyId)
            .HasConstraintName("users_default_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DefaultBranch)
            .WithMany()
            .HasForeignKey(x => x.DefaultBranchId)
            .HasConstraintName("users_default_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("users_department_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DirectManager)
            .WithMany()
            .HasForeignKey(x => x.DirectManagerId)
            .HasConstraintName("users_direct_manager_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.AvatarFile)
            .WithMany()
            .HasForeignKey(x => x.AvatarFileId)
            .HasConstraintName("fk_users_avatar")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
