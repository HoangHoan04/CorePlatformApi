// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho bảng <c>smtp_configs</c>.
/// </summary>
public sealed class SmtpConfigConfiguration : IEntityTypeConfiguration<SmtpConfig>
{
    public void Configure(EntityTypeBuilder<SmtpConfig> builder)
    {
        builder.ToTable("smtp_configs", tb =>
        {
            tb.HasComment("Cấu hình SMTP theo hệ thống, tenant hoặc từng công ty");
            tb.HasCheckConstraint("smtp_configs_provider_check", "provider IN ('SMTP','SES','SENDGRID','MAILGUN','GMAIL_API','M365')");
            tb.HasCheckConstraint("smtp_configs_security_check", "security IN ('NONE','SSL','STARTTLS')");
        });

        builder.HasKey(x => x.Id).HasName("smtp_configs_pkey");

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
            .HasComment("Tenant (NULL = SMTP hệ thống)");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Công ty dùng SMTP riêng");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên cấu hình");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'SMTP'")
            .ValueGeneratedNever()
            .HasComment("Cách gửi");
        builder.Property(x => x.Host)
            .HasColumnName("host")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .HasComment("SMTP host");
        builder.Property(x => x.Port)
            .HasColumnName("port")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .HasComment("SMTP port (587, 465)");
        builder.Property(x => x.Security)
            .HasColumnName("security")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'STARTTLS'")
            .ValueGeneratedNever()
            .HasComment("Mã hóa kết nối");
        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(8)
            .HasComment("Tài khoản");
        builder.Property(x => x.PasswordEncrypted)
            .HasColumnName("password_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("Mật khẩu/API key đã mã hóa");
        builder.Property(x => x.FromEmail)
            .HasColumnName("from_email")
            .HasColumnType("citext")
            .HasColumnOrder(10)
            .IsRequired()
            .HasComment("Email người gửi");
        builder.Property(x => x.FromName)
            .HasColumnName("from_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(11)
            .HasComment("Tên người gửi");
        builder.Property(x => x.ReplyTo)
            .HasColumnName("reply_to")
            .HasColumnType("citext")
            .HasColumnOrder(12)
            .HasComment("Reply-To");
        builder.Property(x => x.RateLimitPerMinute)
            .HasColumnName("rate_limit_per_minute")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .HasComment("Giới hạn số email/phút");
        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Mặc định trong phạm vi");
        builder.Property(x => x.VerifiedAt)
            .HasColumnName("verified_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Lần test gửi thành công");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
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
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
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


        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("smtp_configs_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("smtp_configs_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
