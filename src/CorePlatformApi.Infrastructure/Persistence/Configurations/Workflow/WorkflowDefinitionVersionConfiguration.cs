// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho bảng <c>definition_versions</c>.
/// </summary>
public sealed class WorkflowDefinitionVersionConfiguration : IEntityTypeConfiguration<WorkflowDefinitionVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionVersion> builder)
    {
        builder.ToTable("definition_versions", tb =>
        {
            tb.HasComment("Phiên bản quy trình. Instance đang chạy giữ nguyên phiên bản cũ khi admin sửa quy trình");
            tb.HasCheckConstraint("definition_versions_status_check", "status IN ('DRAFT','PUBLISHED','ARCHIVED')");
        });

        builder.HasKey(x => x.Id).HasName("definition_versions_pkey");

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
        builder.Property(x => x.DefinitionId)
            .HasColumnName("definition_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Quy trình");
        builder.Property(x => x.VersionNo)
            .HasColumnName("version_no")
            .HasColumnType("int")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Số phiên bản");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'DRAFT'")
            .ValueGeneratedNever()
            .HasComment("Nháp, đã phát hành, lưu trữ");
        builder.Property(x => x.DesignerJson)
            .HasColumnName("designer_json")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .HasComment("Dữ liệu sơ đồ trên designer (vị trí node, cạnh) để vẽ lại UI");
        builder.Property(x => x.ChangeNote)
            .HasColumnName("change_note")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Ghi chú thay đổi");
        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Phát hành lúc");
        builder.Property(x => x.PublishedBy)
            .HasColumnName("published_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người phát hành");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.DefinitionId, x.VersionNo }, "uq_wf_versions").IsUnique();

        builder.HasOne(x => x.Definition)
            .WithMany(p => p.WorkflowDefinitionVersions)
            .HasForeignKey(x => x.DefinitionId)
            .HasConstraintName("definition_versions_definition_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
