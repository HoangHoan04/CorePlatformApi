// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho <c>workflow.definition_versions</c>.
/// </summary>
public sealed class WorkflowDefinitionVersionConfiguration : IEntityTypeConfiguration<WorkflowDefinitionVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionVersion> builder)
    {
        builder.ToTable("definition_versions", "workflow", tb =>
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
        builder.Property(x => x.DefinitionId)
            .HasColumnName("definition_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Quy trình");
        builder.Property(x => x.VersionNo)
            .HasColumnName("version_no")
            .HasColumnType("int")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Số phiên bản");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'DRAFT'")
            .ValueGeneratedNever()
            .HasComment("Nháp, đã phát hành, lưu trữ");
        builder.Property(x => x.DesignerJson)
            .HasColumnName("designer_json")
            .HasColumnType("jsonb")
            .HasColumnOrder(4)
            .HasComment("Dữ liệu sơ đồ trên designer (vị trí node, cạnh) để vẽ lại UI");
        builder.Property(x => x.ChangeNote)
            .HasColumnName("change_note")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .HasComment("Ghi chú thay đổi");
        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Phát hành lúc");
        builder.Property(x => x.PublishedBy)
            .HasColumnName("published_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người phát hành");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.DefinitionId, x.VersionNo }, "uq_wf_versions").IsUnique();

        builder.HasOne(x => x.Definition)
            .WithMany(p => p.WorkflowDefinitionVersions)
            .HasForeignKey(x => x.DefinitionId)
            .HasConstraintName("definition_versions_definition_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
