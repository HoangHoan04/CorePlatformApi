// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>user_org_assignments</c>.
/// </summary>
public sealed class UserOrgAssignmentConfiguration : IEntityTypeConfiguration<UserOrgAssignment>
{
    public void Configure(EntityTypeBuilder<UserOrgAssignment> builder)
    {
        builder.ToTable("user_org_assignments", tb =>
        {
            tb.HasComment("Gán user vào nhiều công ty/chi nhánh/phòng ban (kiêm nhiệm). Dùng để tính Data Scope và chọn ngữ cảnh làm việc");
        });

        builder.HasKey(x => x.Id).HasName("user_org_assignments_pkey");

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
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Công ty");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Chi nhánh");
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Phòng ban");
        builder.Property(x => x.JobTitle)
            .HasColumnName("job_title")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(6)
            .HasComment("Chức danh tại đơn vị này");
        builder.Property(x => x.IsPrimary)
            .HasColumnName("is_primary")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Đơn vị chính");
        builder.Property(x => x.IsManager)
            .HasColumnName("is_manager")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Là quản lý của đơn vị này");
        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from")
            .HasColumnType("date")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("current_date")
            .ValueGeneratedNever()
            .HasComment("Hiệu lực từ ngày");
        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to")
            .HasColumnType("date")
            .HasColumnOrder(10)
            .HasComment("Hiệu lực đến ngày (NULL = vô thời hạn)");
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

        builder.HasIndex(x => x.UserId, "ix_user_org_user");
        builder.HasIndex(x => x.DepartmentId, "ix_user_org_dept");
        builder.HasIndex(x => x.UserId, "uq_user_org_primary")
            .IsUnique()
            .HasFilter("is_primary");

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserOrgAssignments)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_org_assignments_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("user_org_assignments_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("user_org_assignments_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("user_org_assignments_department_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
