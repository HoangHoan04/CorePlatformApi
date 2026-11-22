// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.document_types</c>.
/// </summary>
public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("document_types", "mdm", tb =>
        {
            tb.HasComment("Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo");
        });

        builder.HasKey(x => x.Code).HasName("document_types_pkey");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Mã loại chứng từ (HRM_EMPLOYEE, HRM_LEAVE_REQUEST, WMS_GOODS_RECEIPT, CRM_QUOTATION)");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Phân hệ sở hữu");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên loại chứng từ");
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasColumnOrder(3)
            .HasComment("Mô tả");
        builder.Property(x => x.EntityTable)
            .HasColumnName("entity_table")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .HasComment("Bảng dữ liệu gốc bên phân hệ (tham khảo)");
        builder.Property(x => x.SupportsWorkflow)
            .HasColumnName("supports_workflow")
            .HasColumnType("boolean")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Có thể gắn quy trình duyệt");
        builder.Property(x => x.SupportsNumbering)
            .HasColumnName("supports_numbering")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Có sinh mã tự động");
        builder.Property(x => x.SupportsAttachment)
            .HasColumnName("supports_attachment")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Cho phép đính kèm file");
        builder.Property(x => x.WorkflowVariables)
            .HasColumnName("workflow_variables")
            .HasColumnType("jsonb")
            .HasColumnOrder(8)
            .HasComment("Khai báo biến dùng trong điều kiện rẽ nhánh, VD: [{\"name\":\"amount\",\"type\":\"number\",\"label\":\"Tổng tiền\"}]");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");


        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("document_types_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
