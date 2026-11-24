// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile)
/// </summary>
/// <remarks>Bảng <c>users</c> (phân hệ iam); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class User : BaseEntity
{
    /// <summary>
    /// Tên đăng nhập (duy nhất trong tenant)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email (đăng nhập, khôi phục mật khẩu)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Thời điểm xác thực email
    /// </summary>
    public DateTimeOffset? EmailVerifiedAt { get; set; }

    /// <summary>
    /// Số điện thoại (định dạng E.164 khuyến nghị)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Thời điểm xác thực SĐT
    /// </summary>
    public DateTimeOffset? PhoneVerifiedAt { get; set; }

    /// <summary>
    /// Hash mật khẩu (Argon2id/BCrypt/PBKDF2). NULL nếu chỉ đăng nhập SSO ngoài
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Lần đổi mật khẩu gần nhất (tính hết hạn mật khẩu)
    /// </summary>
    public DateTimeOffset? PasswordChangedAt { get; set; }

    /// <summary>
    /// Ép đổi mật khẩu ở lần đăng nhập kế tiếp (lần đầu / sau reset)
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool MustChangePassword { get; set; } = true;

    /// <summary>
    /// Đổi giá trị khi đổi mật khẩu/quyền/khóa → vô hiệu toàn bộ token cũ
    /// </summary>
    /// <remarks>Mặc định DB: <c>encode(gen_random_bytes(16), 'hex')</c></remarks>
    public string SecurityStamp { get; set; } = Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));

    /// <summary>
    /// Họ tên đầy đủ
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Ảnh đại diện (storage.files)
    /// </summary>
    public Guid? AvatarFileId { get; set; }

    /// <summary>
    /// Giới tính (phục vụ xưng hô trong thông báo)
    /// </summary>
    /// <remarks>Ràng buộc: <c>gender IN ('MALE','FEMALE','OTHER')</c></remarks>
    public string? Gender { get; set; }

    /// <summary>
    /// Ngày sinh
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Liên kết hồ sơ nhân viên bên HRM (không FK vì khác service)
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Mã nhân viên (denormalize để hiển thị)
    /// </summary>
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Công ty mặc định khi đăng nhập
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? DefaultCompanyId { get; set; }

    /// <summary>
    /// Chi nhánh mặc định
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? DefaultBranchId { get; set; }

    /// <summary>
    /// Phòng ban chính (OWN/DEPARTMENT scope)
    /// </summary>
    /// <remarks>FK → <c>org.departments(id)</c></remarks>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Quản lý trực tiếp (workflow approver DIRECT_MANAGER)
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid? DirectManagerId { get; set; }

    /// <summary>
    /// Chức danh hiển thị
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Ngôn ngữ ưu tiên (NULL = theo tenant)
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// Múi giờ ưu tiên
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Loại: nhân viên, đối tác, khách hàng portal, tài khoản dịch vụ
    /// </summary>
    /// <remarks>Ràng buộc: <c>user_type IN ('INTERNAL','PARTNER','CUSTOMER','SERVICE')</c>; Mặc định DB: <c>'INTERNAL'</c></remarks>
    public string UserType { get; set; } = "INTERNAL";

    /// <summary>
    /// Quản trị viên tenant (bỏ qua RBAC trong tenant)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsTenantAdmin { get; set; } = false;

    /// <summary>
    /// Quản trị viên nền tảng (đa tenant)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsPlatformAdmin { get; set; } = false;

    /// <summary>
    /// Trạng thái: chờ kích hoạt, hoạt động, bị khóa, vô hiệu
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','ACTIVE','LOCKED','DISABLED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Lý do khóa
    /// </summary>
    public string? LockedReason { get; set; }

    /// <summary>
    /// Thời điểm khóa
    /// </summary>
    public DateTimeOffset? LockedAt { get; set; }

    /// <summary>
    /// Khóa tạm đến thời điểm (do nhập sai mật khẩu); NULL = khóa vô thời hạn
    /// </summary>
    public DateTimeOffset? LockedUntil { get; set; }

    /// <summary>
    /// Người khóa (NULL = hệ thống tự khóa)
    /// </summary>
    public Guid? LockedBy { get; set; }

    /// <summary>
    /// Số lần đăng nhập sai liên tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>
    /// Lần đăng nhập sai gần nhất
    /// </summary>
    public DateTimeOffset? LastFailedLoginAt { get; set; }

    /// <summary>
    /// Lần đăng nhập thành công gần nhất
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// IP đăng nhập gần nhất
    /// </summary>
    public IPAddress? LastLoginIp { get; set; }

    /// <summary>
    /// Bật xác thực 2 lớp
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool MfaEnabled { get; set; } = false;

    /// <summary>
    /// Phương thức 2FA
    /// </summary>
    /// <remarks>Ràng buộc: <c>mfa_type IN ('TOTP','EMAIL_OTP','SMS_OTP')</c></remarks>
    public string? MfaType { get; set; }

    /// <summary>
    /// Secret TOTP đã mã hóa
    /// </summary>
    public string? MfaSecretEncrypted { get; set; }

    /// <summary>
    /// Số thiết bị đăng nhập đồng thời tối đa (NULL = theo chính sách tenant)
    /// </summary>
    public int? MaxDevices { get; set; }

    /// <summary>
    /// Thời điểm kích hoạt tài khoản
    /// </summary>
    public DateTimeOffset? ActivatedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>default_company_id</c>).
    /// </summary>
    public Company? DefaultCompany { get; set; }

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>default_branch_id</c>).
    /// </summary>
    public Branch? DefaultBranch { get; set; }

    /// <summary>
    /// Điều hướng tới Phòng ban / bộ phận / tổ nhóm dạng cây (qua <c>department_id</c>).
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>direct_manager_id</c>).
    /// </summary>
    public User? DirectManager { get; set; }

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>avatar_file_id</c>).
    /// </summary>
    public StorageFile? AvatarFile { get; set; }

    /// <summary>
    /// Danh sách UserOrgAssignment con (qua <c>iam.user_org_assignments.user_id</c>).
    /// </summary>
    public ICollection<UserOrgAssignment> UserOrgAssignments { get; set; } = [];

    /// <summary>
    /// Danh sách UserRole con (qua <c>iam.user_roles.user_id</c>).
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// Danh sách UserPermissionOverride con (qua <c>iam.user_permission_overrides.user_id</c>).
    /// </summary>
    public ICollection<UserPermissionOverride> UserPermissionOverrides { get; set; } = [];

    /// <summary>
    /// Danh sách UserExternalLogin con (qua <c>iam.user_external_logins.user_id</c>).
    /// </summary>
    public ICollection<UserExternalLogin> UserExternalLogins { get; set; } = [];

    /// <summary>
    /// Danh sách UserRecoveryCode con (qua <c>iam.user_recovery_codes.user_id</c>).
    /// </summary>
    public ICollection<UserRecoveryCode> UserRecoveryCodes { get; set; } = [];

    /// <summary>
    /// Danh sách PasswordHistory con (qua <c>iam.password_histories.user_id</c>).
    /// </summary>
    public ICollection<PasswordHistory> PasswordHistories { get; set; } = [];

    /// <summary>
    /// Danh sách VerificationToken con (qua <c>iam.verification_tokens.user_id</c>).
    /// </summary>
    public ICollection<VerificationToken> VerificationTokens { get; set; } = [];

    /// <summary>
    /// Danh sách UserDevice con (qua <c>iam.user_devices.user_id</c>).
    /// </summary>
    public ICollection<UserDevice> UserDevices { get; set; } = [];

    /// <summary>
    /// Danh sách UserSession con (qua <c>iam.user_sessions.user_id</c>).
    /// </summary>
    public ICollection<UserSession> UserSessions { get; set; } = [];

    /// <summary>
    /// Danh sách RefreshToken con (qua <c>iam.refresh_tokens.user_id</c>).
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>
    /// Danh sách DeviceToken con (qua <c>notify.device_tokens.user_id</c>).
    /// </summary>
    public ICollection<DeviceToken> DeviceTokens { get; set; } = [];

    /// <summary>
    /// Danh sách UserNotification con (qua <c>notify.user_notifications.user_id</c>).
    /// </summary>
    public ICollection<UserNotification> UserNotifications { get; set; } = [];

    /// <summary>
    /// Danh sách UserNotificationPreference con (qua <c>notify.user_preferences.user_id</c>).
    /// </summary>
    public ICollection<UserNotificationPreference> UserNotificationPreferences { get; set; } = [];

    /// <summary>
    /// Danh sách AiConversation con (qua <c>ai.conversations.user_id</c>).
    /// </summary>
    public ICollection<AiConversation> AiConversations { get; set; } = [];

    #endregion
}
