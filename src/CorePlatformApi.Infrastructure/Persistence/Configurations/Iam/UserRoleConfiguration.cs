// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.user_roles</c>.
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "iam", tb =>
        {
            tb.HasComment("Gán vai trò cho user, có thể giới hạn vai trò trong một công ty/chi nhánh và có thời hạn");
        });

        builder.HasKey(x => x.Id).HasName("user_roles_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Vai trò");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Vai trò chỉ có hiệu lực trong công ty này (NULL = mọi công ty)");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Vai trò chỉ có hiệu lực trong chi nhánh này");
        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Hiệu lực từ");
        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Hiệu lực đến (gán quyền tạm thời)");
        builder.Property(x => x.GrantedBy)
            .HasColumnName("granted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người cấp quyền");
        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Ghi chú lý do cấp");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.UserId, x.RoleId, x.CompanyId, x.BranchId }, "uq_user_roles")
            .IsUnique()
            .AreNullsDistinct(false);
        builder.HasIndex(x => x.RoleId, "ix_user_roles_role");

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserRoles)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_roles_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Role)
            .WithMany(p => p.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("user_roles_role_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("user_roles_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("user_roles_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
