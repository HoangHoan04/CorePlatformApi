// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Org;

/// <summary>
/// Cấu hình EF Core cho bảng <c>departments</c>.
/// </summary>
public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments", tb =>
        {
            tb.HasComment("Phòng ban / bộ phận / tổ nhóm dạng cây. Dùng cho Data Scope DEPARTMENT / DEPARTMENT_TREE");
            tb.HasCheckConstraint("departments_dept_type_check", "dept_type IN ('DIVISION','DEPARTMENT','TEAM','GROUP')");
            tb.HasCheckConstraint("departments_status_check", "status IN ('ACTIVE','INACTIVE')");
        });

        builder.HasKey(x => x.Id).HasName("departments_pkey");

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
            .IsRequired()
            .HasComment("Công ty");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Chi nhánh (NULL nếu phòng ban cấp công ty)");
        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Phòng ban cha");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Mã phòng ban");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Tên phòng ban");
        builder.Property(x => x.DeptType)
            .HasColumnName("dept_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'DEPARTMENT'")
            .ValueGeneratedNever()
            .HasComment("Loại: khối, phòng, tổ, nhóm");
        builder.Property(x => x.ManagerUserId)
            .HasColumnName("manager_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Trưởng bộ phận (iam.users) — dùng cho workflow approver DEPARTMENT_MANAGER");
        builder.Property(x => x.DeputyUserId)
            .HasColumnName("deputy_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Phó bộ phận");
        builder.Property(x => x.CostCenterCode)
            .HasColumnName("cost_center_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(10)
            .HasComment("Mã trung tâm chi phí (đối soát kế toán)");
        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasColumnType("text")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'/'")
            .ValueGeneratedNever()
            .HasComment("Đường dẫn id '/root/child/' — truy vấn cây con bằng LIKE path || '%'");
        builder.Property(x => x.Level)
            .HasColumnName("level")
            .HasColumnType("smallint")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Cấp trong cây");
        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Thứ tự hiển thị");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => x.ParentId, "ix_departments_parent");
        builder.HasIndex(x => x.Path, "ix_departments_path")
            .HasOperators("text_pattern_ops");
        builder.HasIndex(x => x.BranchId, "ix_departments_branch")
            .HasFilter("NOT is_deleted");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("departments_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("departments_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("departments_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("departments_parent_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ManagerUser)
            .WithMany()
            .HasForeignKey(x => x.ManagerUserId)
            .HasConstraintName("fk_departments_manager")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DeputyUser)
            .WithMany()
            .HasForeignKey(x => x.DeputyUserId)
            .HasConstraintName("fk_departments_deputy")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
