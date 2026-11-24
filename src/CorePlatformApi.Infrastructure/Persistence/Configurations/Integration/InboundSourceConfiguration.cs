// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Integration;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho bảng <c>inbound_sources</c>.
/// </summary>
public sealed class InboundSourceConfiguration : IEntityTypeConfiguration<InboundSource>
{
    public void Configure(EntityTypeBuilder<InboundSource> builder)
    {
        builder.ToTable("inbound_sources", tb =>
        {
            tb.HasComment("Nguồn webhook INBOUND: cổng thanh toán (VNPay, SePay, VietQR), sàn TMĐT, máy chấm công, hóa đơn điện tử");
            tb.HasCheckConstraint("inbound_sources_provider_check", "provider IN ('VNPAY','SEPAY','VIETQR','MOMO','PAYOS','SHOPEE','TIKTOK_SHOP','LAZADA','TIKI','ZALO_OA','ATTENDANCE_DEVICE','EINVOICE','BANK','CUSTOM')");
            tb.HasCheckConstraint("inbound_sources_auth_type_check", "auth_type IN ('NONE','HMAC','API_KEY','BASIC','CHECKSUM','JWT','IP_ONLY')");
            tb.HasCheckConstraint("inbound_sources_status_check", "status IN ('ACTIVE','PAUSED','DISABLED')");
        });

        builder.HasKey(x => x.Id).HasName("inbound_sources_pkey");

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
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã nguồn (SEPAY_MAIN, SHOPEE_STORE1)");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Nhà cung cấp");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên hiển thị");
        builder.Property(x => x.EndpointToken)
            .HasColumnName("endpoint_token")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Token ngẫu nhiên trong URL nhận: /integration/inbound/{endpoint_token}");
        builder.Property(x => x.AuthType)
            .HasColumnName("auth_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'HMAC'")
            .ValueGeneratedNever()
            .HasComment("Cách xác thực request đến");
        builder.Property(x => x.SecretEncrypted)
            .HasColumnName("secret_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Secret/checksum key đã mã hóa");
        builder.Property(x => x.SignatureHeader)
            .HasColumnName("signature_header")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(8)
            .HasComment("Header chứa chữ ký");
        builder.Property(x => x.SignatureAlgorithm)
            .HasColumnName("signature_algorithm")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(9)
            .HasComment("Thuật toán (HMAC_SHA256, HMAC_SHA512, MD5)");
        builder.Property(x => x.IpWhitelist)
            .HasColumnName("ip_whitelist")
            .HasColumnType("cidr[]")
            .HasColumnOrder(10)
            .HasComment("IP nguồn được phép");
        builder.Property(x => x.TargetEventType)
            .HasColumnName("target_event_type")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(11)
            .HasComment("Event nội bộ phát ra sau khi xác thực (integration.event_types)");
        builder.Property(x => x.MappingScript)
            .HasColumnName("mapping_script")
            .HasColumnType("text")
            .HasColumnOrder(12)
            .HasComment("Mapping payload ngoài → payload nội bộ (JSONata/Liquid)");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Công ty nhận dữ liệu");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Chi nhánh nhận dữ liệu (máy chấm công tại chi nhánh)");
        builder.Property(x => x.Config)
            .HasColumnName("config")
            .HasColumnType("jsonb")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Cấu hình riêng (số tài khoản nhận tiền, merchant id...)");
        builder.Property(x => x.LastReceivedAt)
            .HasColumnName("last_received_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Lần nhận gần nhất");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => x.EndpointToken, "inbound_sources_endpoint_token_key").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("inbound_sources_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("inbound_sources_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("inbound_sources_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
