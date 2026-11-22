// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho <c>gateway.error_codes</c>.
/// </summary>
public sealed class ErrorCodeConfiguration : IEntityTypeConfiguration<ErrorCode>
{
    public void Configure(EntityTypeBuilder<ErrorCode> builder)
    {
        builder.ToTable("error_codes", "gateway", tb =>
        {
            tb.HasComment("Mã lỗi chuẩn hóa toàn hệ sinh thái. Response lỗi: {success:false, code, message, traceId, errors[]}");
            tb.HasCheckConstraint("error_codes_http_status_check", "http_status BETWEEN 400 AND 599");
            tb.HasCheckConstraint("error_codes_category_check", "category IN ('VALIDATION','AUTHENTICATION','AUTHORIZATION','NOT_FOUND','CONFLICT','BUSINESS','RATE_LIMIT','INTEGRATION','SYSTEM')");
        });

        builder.HasKey(x => x.Code).HasName("error_codes_pkey");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Mã lỗi (AUTH_TOKEN_EXPIRED, HRM_LEAVE_BALANCE_INSUFFICIENT)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Phân hệ");
        builder.Property(x => x.HttpStatus)
            .HasColumnName("http_status")
            .HasColumnType("smallint")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("HTTP status tương ứng");
        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Nhóm lỗi");
        builder.Property(x => x.MessageKey)
            .HasColumnName("message_key")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .HasComment("Khóa i18n của thông báo (config.translation_keys)");
        builder.Property(x => x.DefaultMessage)
            .HasColumnName("default_message")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Thông báo mặc định tiếng Việt");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Giải thích cho dev / hướng xử lý");
        builder.Property(x => x.IsRetryable)
            .HasColumnName("is_retryable")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Client có nên thử lại");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");


        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("error_codes_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
