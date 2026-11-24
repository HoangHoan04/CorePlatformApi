// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>gateway_clients</c>.
/// </summary>
public sealed class AiGatewayClientConfiguration : IEntityTypeConfiguration<AiGatewayClient>
{
    public void Configure(EntityTypeBuilder<AiGatewayClient> builder)
    {
        builder.ToTable("gateway_clients", tb =>
        {
            tb.HasComment("Client nội bộ gọi AI Gateway (mỗi phân hệ/ứng dụng một client key) để định danh, rate limit, tính quota");
            tb.HasCheckConstraint("gateway_clients_status_check", "status IN ('ACTIVE','DISABLED','REVOKED')");
        });

        builder.HasKey(x => x.Id).HasName("gateway_clients_pkey");

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
            .HasComment("Tenant (NULL = dùng chung)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Phân hệ");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên client (hrm-api, crm-api)");
        builder.Property(x => x.KeyPrefix)
            .HasColumnName("key_prefix")
            .HasColumnType("varchar(16)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tiền tố key hiển thị");
        builder.Property(x => x.KeyHash)
            .HasColumnName("key_hash")
            .HasColumnType("varchar(128)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Hash key nội bộ");
        builder.Property(x => x.AllowedAliases)
            .HasColumnName("allowed_aliases")
            .HasColumnType("text[]")
            .HasColumnOrder(6)
            .HasComment("Alias model được phép dùng (NULL = tất cả)");
        builder.Property(x => x.RequestsPerMinute)
            .HasColumnName("requests_per_minute")
            .HasColumnType("int")
            .HasColumnOrder(7)
            .HasComment("Rate limit RPM");
        builder.Property(x => x.TokensPerMinute)
            .HasColumnName("tokens_per_minute")
            .HasColumnType("int")
            .HasColumnOrder(8)
            .HasComment("Rate limit TPM");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Dùng gần nhất");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.KeyPrefix, "gateway_clients_key_prefix_key").IsUnique();
        builder.HasIndex(x => x.KeyHash, "gateway_clients_key_hash_key").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("gateway_clients_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("gateway_clients_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
