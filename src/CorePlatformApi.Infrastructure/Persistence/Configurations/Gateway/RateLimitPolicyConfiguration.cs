// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Gateway;

/// <summary>
/// Cấu hình EF Core cho <c>gateway.rate_limit_policies</c>.
/// </summary>
public sealed class RateLimitPolicyConfiguration : IEntityTypeConfiguration<RateLimitPolicy>
{
    public void Configure(EntityTypeBuilder<RateLimitPolicy> builder)
    {
        builder.ToTable("rate_limit_policies", "gateway", tb =>
        {
            tb.HasComment("Chính sách rate limit/throttling theo IP, user, tenant, client, API key");
            tb.HasCheckConstraint("rate_limit_policies_partition_by_check", "partition_by IN ('IP','USER','TENANT','CLIENT','API_KEY','GLOBAL')");
            tb.HasCheckConstraint("rate_limit_policies_algorithm_check", "algorithm IN ('FIXED_WINDOW','SLIDING_WINDOW','TOKEN_BUCKET','CONCURRENCY')");
            tb.HasCheckConstraint("rate_limit_policies_permit_limit_check", "permit_limit > 0");
        });

        builder.HasKey(x => x.Id).HasName("rate_limit_policies_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã chính sách (default-user, login-strict, partner-basic)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.PartitionBy)
            .HasColumnName("partition_by")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Đếm theo khóa nào");
        builder.Property(x => x.Algorithm)
            .HasColumnName("algorithm")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'SLIDING_WINDOW'")
            .ValueGeneratedNever()
            .HasComment("Thuật toán");
        builder.Property(x => x.PermitLimit)
            .HasColumnName("permit_limit")
            .HasColumnType("int")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Số request cho phép trong cửa sổ");
        builder.Property(x => x.WindowSeconds)
            .HasColumnName("window_seconds")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("60")
            .ValueGeneratedNever()
            .HasComment("Độ dài cửa sổ (giây)");
        builder.Property(x => x.Segments)
            .HasColumnName("segments")
            .HasColumnType("smallint")
            .HasColumnOrder(7)
            .HasComment("Số phân đoạn (SLIDING_WINDOW)");
        builder.Property(x => x.TokensPerPeriod)
            .HasColumnName("tokens_per_period")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .HasComment("Token nạp lại mỗi chu kỳ (TOKEN_BUCKET)");
        builder.Property(x => x.QueueLimit)
            .HasColumnName("queue_limit")
            .HasColumnType("int")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số request được xếp hàng khi vượt");
        builder.Property(x => x.RejectStatus)
            .HasColumnName("reject_status")
            .HasColumnType("smallint")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("429")
            .ValueGeneratedNever()
            .HasComment("HTTP status khi từ chối");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang dùng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.Code, "rate_limit_policies_code_key").IsUnique();
    }
}
