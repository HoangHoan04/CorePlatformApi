// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>prompt_guardrails</c>.
/// </summary>
public sealed class AiPromptGuardrailConfiguration : IEntityTypeConfiguration<AiPromptGuardrail>
{
    public void Configure(EntityTypeBuilder<AiPromptGuardrail> builder)
    {
        builder.ToTable("prompt_guardrails", tb =>
        {
            tb.HasComment("Gắn guardrail vào phiên bản prompt");
        });

        builder.HasKey(x => x.Id).HasName("prompt_guardrails_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (UUID v7)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.PromptVersionId)
            .HasColumnName("prompt_version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phiên bản prompt");
        builder.Property(x => x.GuardrailId)
            .HasColumnName("guardrail_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Guardrail");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.PromptVersionId, x.GuardrailId }, "prompt_guardrails_prompt_version_id_guardrail_id_key").IsUnique();

        builder.HasOne(x => x.PromptVersion)
            .WithMany(p => p.AiPromptGuardrails)
            .HasForeignKey(x => x.PromptVersionId)
            .HasConstraintName("prompt_guardrails_prompt_version_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Guardrail)
            .WithMany(p => p.AiPromptGuardrails)
            .HasForeignKey(x => x.GuardrailId)
            .HasConstraintName("prompt_guardrails_guardrail_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
