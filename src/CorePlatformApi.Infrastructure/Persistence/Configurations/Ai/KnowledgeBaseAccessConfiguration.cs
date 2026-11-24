// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>knowledge_base_access</c>.
/// </summary>
public sealed class KnowledgeBaseAccessConfiguration : IEntityTypeConfiguration<KnowledgeBaseAccess>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseAccess> builder)
    {
        builder.ToTable("knowledge_base_access", tb =>
        {
            tb.HasComment("Phân quyền truy cập kho tri thức theo vai trò/phòng ban/chi nhánh/user");
            tb.HasCheckConstraint("knowledge_base_access_principal_type_check", "principal_type IN ('ROLE','DEPARTMENT','BRANCH','COMPANY','USER')");
        });

        builder.HasKey(x => x.Id).HasName("knowledge_base_access_pkey");

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
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.KnowledgeBaseId)
            .HasColumnName("knowledge_base_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Kho tri thức");
        builder.Property(x => x.PrincipalType)
            .HasColumnName("principal_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Loại đối tượng");
        builder.Property(x => x.PrincipalId)
            .HasColumnName("principal_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Id đối tượng");
        builder.Property(x => x.CanManage)
            .HasColumnName("can_manage")
            .HasColumnType("boolean")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Được quản lý tài liệu");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.KnowledgeBaseId, x.PrincipalType, x.PrincipalId }, "uq_ai_kb_access").IsUnique();

        builder.HasOne(x => x.KnowledgeBase)
            .WithMany(p => p.KnowledgeBaseAccesses)
            .HasForeignKey(x => x.KnowledgeBaseId)
            .HasConstraintName("knowledge_base_access_knowledge_base_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
