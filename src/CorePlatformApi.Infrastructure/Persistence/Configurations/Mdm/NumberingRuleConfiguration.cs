// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.numbering_rules</c>.
/// </summary>
public sealed class NumberingRuleConfiguration : IEntityTypeConfiguration<NumberingRule>
{
    public void Configure(EntityTypeBuilder<NumberingRule> builder)
    {
        builder.ToTable("numbering_rules", "mdm", tb =>
        {
            tb.HasComment("Quy tắc sinh mã tự động theo loại chứng từ, có thể riêng theo công ty/chi nhánh. Pattern hỗ trợ token {YYYY} {YY} {MM} {DD} {YYMM} {BRANCH} {COMPANY} {0001}");
            tb.HasCheckConstraint("numbering_rules_padding_length_check", "padding_length BETWEEN 1 AND 12");
            tb.HasCheckConstraint("numbering_rules_step_check", "step > 0");
            tb.HasCheckConstraint("numbering_rules_reset_policy_check", "reset_policy IN ('NEVER','YEARLY','MONTHLY','DAILY')");
            tb.HasCheckConstraint("numbering_rules_counter_scope_check", "counter_scope IN ('RULE','COMPANY','BRANCH')");
        });

        builder.HasKey(x => x.Id).HasName("numbering_rules_pkey");

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
        builder.Property(x => x.DocumentTypeCode)
            .HasColumnName("document_type_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại chứng từ");
        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Áp dụng riêng công ty (NULL = toàn tenant)");
        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Áp dụng riêng chi nhánh (ưu tiên cao nhất)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tên quy tắc");
        builder.Property(x => x.Pattern)
            .HasColumnName("pattern")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(6)
            .HasComment("Mẫu đầy đủ, VD: 'PNK-{BRANCH}-{00001}', 'NV-{YYYY}-{0001}'. Nếu NULL thì ghép prefix + số + suffix");
        builder.Property(x => x.Prefix)
            .HasColumnName("prefix")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(7)
            .HasComment("Tiền tố (có thể chứa token)");
        builder.Property(x => x.Suffix)
            .HasColumnName("suffix")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(8)
            .HasComment("Hậu tố (có thể chứa token)");
        builder.Property(x => x.PaddingLength)
            .HasColumnName("padding_length")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("4")
            .ValueGeneratedNever()
            .HasComment("Độ dài phần số tự tăng (khi không dùng pattern)");
        builder.Property(x => x.StartValue)
            .HasColumnName("start_value")
            .HasColumnType("bigint")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Giá trị bắt đầu");
        builder.Property(x => x.Step)
            .HasColumnName("step")
            .HasColumnType("int")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Bước tăng");
        builder.Property(x => x.ResetPolicy)
            .HasColumnName("reset_policy")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("'NEVER'")
            .ValueGeneratedNever()
            .HasComment("Chu kỳ reset số đếm");
        builder.Property(x => x.CounterScope)
            .HasColumnName("counter_scope")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("'RULE'")
            .ValueGeneratedNever()
            .HasComment("Số đếm dùng chung theo quy tắc hay tách riêng từng công ty/chi nhánh");
        builder.Property(x => x.AllowManual)
            .HasColumnName("allow_manual")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cho phép người dùng tự nhập mã");
        builder.Property(x => x.Preview)
            .HasColumnName("preview")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(15)
            .HasComment("Ví dụ mã sinh ra (hiển thị UI)");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang áp dụng");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người xóa mềm (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.DocumentTypeCode, x.CompanyId, x.BranchId }, "uq_numbering_rules_scope")
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("is_active AND NOT is_deleted");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("numbering_rules_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeCode)
            .HasConstraintName("numbering_rules_document_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("numbering_rules_company_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("numbering_rules_branch_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
