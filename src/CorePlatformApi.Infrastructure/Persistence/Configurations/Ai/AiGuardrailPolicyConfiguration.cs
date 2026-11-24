// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>guardrail_policies</c>.
/// </summary>
public sealed class AiGuardrailPolicyConfiguration : IEntityTypeConfiguration<AiGuardrailPolicy>
{
    public void Configure(EntityTypeBuilder<AiGuardrailPolicy> builder)
    {
        builder.ToTable("guardrail_policies", tb =>
        {
            tb.HasComment("Chính sách guardrail: che PII, chặn chủ đề, phát hiện prompt injection, kiểm tra output");
            tb.HasCheckConstraint("guardrail_policies_guard_type_check", "guard_type IN ('PII_MASK','BLOCKED_KEYWORDS','BLOCKED_TOPICS','PROMPT_INJECTION','OUTPUT_SCHEMA','MAX_INPUT_LENGTH','TOXICITY','CUSTOM')");
            tb.HasCheckConstraint("guardrail_policies_apply_on_check", "apply_on IN ('INPUT','OUTPUT','BOTH')");
            tb.HasCheckConstraint("guardrail_policies_action_check", "action IN ('BLOCK','MASK','WARN','LOG')");
        });

        builder.HasKey(x => x.Id).HasName("guardrail_policies_pkey");

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
            .HasComment("Tenant (NULL = hệ thống)");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã chính sách");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.GuardType)
            .HasColumnName("guard_type")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Loại guardrail");
        builder.Property(x => x.ApplyOn)
            .HasColumnName("apply_on")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'INPUT'")
            .ValueGeneratedNever()
            .HasComment("Áp dụng cho đầu vào/đầu ra");
        builder.Property(x => x.Config)
            .HasColumnName("config")
            .HasColumnType("jsonb")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Cấu hình (regex CCCD/SĐT/STK, danh sách từ khóa...)");
        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'BLOCK'")
            .ValueGeneratedNever()
            .HasComment("Hành động khi vi phạm");
        builder.Property(x => x.IsGlobal)
            .HasColumnName("is_global")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Áp dụng cho mọi request của tenant");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang bật");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.TenantId, x.Code }, "uq_ai_guardrails")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("guardrail_policies_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
