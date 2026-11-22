// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.prompt_guardrails</c>.
/// </summary>
public sealed class AiPromptGuardrailConfiguration : IEntityTypeConfiguration<AiPromptGuardrail>
{
    public void Configure(EntityTypeBuilder<AiPromptGuardrail> builder)
    {
        builder.ToTable("prompt_guardrails", "ai", tb =>
        {
            tb.HasComment("Gắn guardrail vào phiên bản prompt");
        });

        builder.HasKey(x => new { x.PromptVersionId, x.GuardrailId }).HasName("prompt_guardrails_pkey");

        builder.Property(x => x.PromptVersionId)
            .HasColumnName("prompt_version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Phiên bản prompt");
        builder.Property(x => x.GuardrailId)
            .HasColumnName("guardrail_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Guardrail");


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
