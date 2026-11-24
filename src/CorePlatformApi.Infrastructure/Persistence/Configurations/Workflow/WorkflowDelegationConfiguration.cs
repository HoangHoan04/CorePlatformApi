// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Workflow;

/// <summary>
/// Cấu hình EF Core cho bảng <c>delegations</c>.
/// </summary>
public sealed class WorkflowDelegationConfiguration : IEntityTypeConfiguration<WorkflowDelegation>
{
    public void Configure(EntityTypeBuilder<WorkflowDelegation> builder)
    {
        builder.ToTable("delegations", tb =>
        {
            tb.HasComment("Ủy quyền duyệt khi vắng mặt (đi công tác, nghỉ phép)");
            tb.HasCheckConstraint("delegations_scope_check", "scope IN ('ALL','MODULE','DOCUMENT_TYPE','DEFINITION')");
            tb.HasCheckConstraint("delegations_status_check", "status IN ('ACTIVE','REVOKED','EXPIRED')");
            tb.HasCheckConstraint("ck_delegation_period", "end_at > start_at");
            tb.HasCheckConstraint("ck_delegation_self", "delegator_id <> delegate_id");
        });

        builder.HasKey(x => x.Id).HasName("delegations_pkey");

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
        builder.Property(x => x.DelegatorId)
            .HasColumnName("delegator_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Người ủy quyền");
        builder.Property(x => x.DelegateId)
            .HasColumnName("delegate_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Người được ủy quyền");
        builder.Property(x => x.Scope)
            .HasColumnName("scope")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'ALL'")
            .ValueGeneratedNever()
            .HasComment("Phạm vi ủy quyền");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(5)
            .HasComment("Phân hệ (scope = MODULE)");
        builder.Property(x => x.DocumentTypeCode)
            .HasColumnName("document_type_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(6)
            .HasComment("Loại chứng từ (scope = DOCUMENT_TYPE)");
        builder.Property(x => x.DefinitionId)
            .HasColumnName("definition_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Quy trình (scope = DEFINITION)");
        builder.Property(x => x.MaxAmount)
            .HasColumnName("max_amount")
            .HasColumnType("numeric(20,2)")
            .HasColumnOrder(8)
            .HasComment("Chỉ ủy quyền chứng từ có giá trị ≤ mức này");
        builder.Property(x => x.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasComment("Bắt đầu");
        builder.Property(x => x.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasComment("Kết thúc");
        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(11)
            .HasComment("Lý do");
        builder.Property(x => x.IncludePending)
            .HasColumnName("include_pending")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Chuyển cả các task đang chờ cho người được ủy quyền");
        builder.Property(x => x.NotifyDelegator)
            .HasColumnName("notify_delegator")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Vẫn gửi thông báo cho người ủy quyền");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thu hồi lúc");
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
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.DelegatorId, x.StartAt, x.EndAt }, "ix_wf_delegations_active")
            .HasFilter("status = 'ACTIVE'");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("delegations_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Delegator)
            .WithMany()
            .HasForeignKey(x => x.DelegatorId)
            .HasConstraintName("delegations_delegator_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Delegate)
            .WithMany()
            .HasForeignKey(x => x.DelegateId)
            .HasConstraintName("delegations_delegate_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("delegations_document_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Definition)
            .WithMany()
            .HasForeignKey(x => x.DefinitionId)
            .HasConstraintName("delegations_definition_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
