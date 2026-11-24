// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho bảng <c>integration_event_types</c>.
/// </summary>
public sealed class IntegrationEventTypeConfiguration : IEntityTypeConfiguration<IntegrationEventType>
{
    public void Configure(EntityTypeBuilder<IntegrationEventType> builder)
    {
        builder.ToTable("integration_event_types", tb =>
        {
            tb.HasComment("Danh mục event nghiệp vụ trao đổi giữa các phân hệ (crm.order.confirmed, hrm.payroll.closed...) kèm schema payload");
        });

        builder.HasKey(x => x.Id).HasName("integration_event_types_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (UUID v7)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã event (domain.entity.action)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Phân hệ phát (producer)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Tên event");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Mô tả");
        builder.Property(x => x.SchemaVersion)
            .HasColumnName("schema_version")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Phiên bản schema payload");
        builder.Property(x => x.PayloadSchema)
            .HasColumnName("payload_schema")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .HasComment("JSON Schema của payload (validate + sinh tài liệu)");
        builder.Property(x => x.ExchangeName)
            .HasColumnName("exchange_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(8)
            .HasComment("Exchange/stream trên RabbitMQ/Redis Streams");
        builder.Property(x => x.RoutingKey)
            .HasColumnName("routing_key")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(9)
            .HasComment("Routing key");
        builder.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho phép đối tác đăng ký qua webhook");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasAlternateKey(x => x.Code).HasName("integration_event_types_code_key");

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("integration_event_types_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
