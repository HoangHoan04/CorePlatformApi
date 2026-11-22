// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Integration;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.connections</c>.
/// </summary>
public sealed class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("connections", "integration", tb =>
        {
            tb.HasComment("Kết nối tới hệ thống bên thứ 3 (lưu credential/OAuth token): sàn TMĐT, nhà cung cấp HĐĐT, ngân hàng, Zalo OA...");
            tb.HasCheckConstraint("connections_auth_type_check", "auth_type IN ('API_KEY','BASIC','OAUTH2','CERTIFICATE','CUSTOM')");
            tb.HasCheckConstraint("connections_status_check", "status IN ('PENDING','ACTIVE','EXPIRED','ERROR','DISABLED')");
        });

        builder.HasKey(x => x.Id).HasName("connections_pkey");

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
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Công ty sở hữu kết nối");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Nhà cung cấp (VNPT_EINVOICE, VIETTEL_SINVOICE, MISA_MEINVOICE, SHOPEE, ZALO_OA, GHN, GHTK...)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên kết nối");
        builder.Property(x => x.AuthType)
            .HasColumnName("auth_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Kiểu xác thực");
        builder.Property(x => x.BaseUrl)
            .HasColumnName("base_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(6)
            .HasComment("URL API");
        builder.Property(x => x.Config)
            .HasColumnName("config")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Cấu hình không bí mật (mẫu số HĐ, ký hiệu, shop_id...)");
        builder.Property(x => x.CredentialsEncrypted)
            .HasColumnName("credentials_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(8)
            .HasComment("Credential đã mã hóa (username/password/api key/chứng thư)");
        builder.Property(x => x.AccessTokenEncrypted)
            .HasColumnName("access_token_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("OAuth access token đã mã hóa");
        builder.Property(x => x.RefreshTokenEncrypted)
            .HasColumnName("refresh_token_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(10)
            .HasComment("OAuth refresh token đã mã hóa");
        builder.Property(x => x.TokenExpiresAt)
            .HasColumnName("token_expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Hạn access token (job tự refresh)");
        builder.Property(x => x.ExternalAccountId)
            .HasColumnName("external_account_id")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(12)
            .HasComment("Id tài khoản phía đối tác (shop_id, oa_id)");
        builder.Property(x => x.LastSyncAt)
            .HasColumnName("last_sync_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Đồng bộ gần nhất");
        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text")
            .HasColumnOrder(14)
            .HasComment("Lỗi gần nhất");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
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
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
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
            .HasComment("Người xóa mềm (iam.users.id)");


        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("connections_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("connections_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
