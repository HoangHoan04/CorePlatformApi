// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>prompt_versions</c>.
/// </summary>
public sealed class AiPromptVersionConfiguration : IEntityTypeConfiguration<AiPromptVersion>
{
    public void Configure(EntityTypeBuilder<AiPromptVersion> builder)
    {
        builder.ToTable("prompt_versions", tb =>
        {
            tb.HasComment("Phiên bản prompt (quản lý thay đổi, rollback, A/B test)");
            tb.HasCheckConstraint("prompt_versions_response_format_check", "response_format IN ('TEXT','JSON','JSON_SCHEMA')");
            tb.HasCheckConstraint("prompt_versions_traffic_percent_check", "traffic_percent BETWEEN 0 AND 100");
            tb.HasCheckConstraint("prompt_versions_status_check", "status IN ('DRAFT','PUBLISHED','ARCHIVED')");
        });

        builder.HasKey(x => x.Id).HasName("prompt_versions_pkey");

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
        builder.Property(x => x.TemplateId)
            .HasColumnName("template_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Prompt");
        builder.Property(x => x.VersionNo)
            .HasColumnName("version_no")
            .HasColumnType("int")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Số phiên bản");
        builder.Property(x => x.SystemPrompt)
            .HasColumnName("system_prompt")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .HasComment("System prompt");
        builder.Property(x => x.UserPrompt)
            .HasColumnName("user_prompt")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("User prompt có biến {{variable}}");
        builder.Property(x => x.Variables)
            .HasColumnName("variables")
            .HasColumnType("jsonb")
            .HasColumnOrder(6)
            .HasComment("Khai báo biến đầu vào, VD: [{\"name\":\"cv_text\",\"required\":true}]");
        builder.Property(x => x.ModelAliasId)
            .HasColumnName("model_alias_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Alias model dùng");
        builder.Property(x => x.Temperature)
            .HasColumnName("temperature")
            .HasColumnType("numeric(3,2)")
            .HasColumnOrder(8)
            .HasComment("Nhiệt độ");
        builder.Property(x => x.TopP)
            .HasColumnName("top_p")
            .HasColumnType("numeric(3,2)")
            .HasColumnOrder(9)
            .HasComment("Top-p");
        builder.Property(x => x.MaxOutputTokens)
            .HasColumnName("max_output_tokens")
            .HasColumnType("int")
            .HasColumnOrder(10)
            .HasComment("Giới hạn token đầu ra");
        builder.Property(x => x.ResponseFormat)
            .HasColumnName("response_format")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'TEXT'")
            .ValueGeneratedNever()
            .HasComment("Định dạng trả về");
        builder.Property(x => x.JsonSchema)
            .HasColumnName("json_schema")
            .HasColumnType("jsonb")
            .HasColumnOrder(12)
            .HasComment("JSON Schema đầu ra (JSON_SCHEMA)");
        builder.Property(x => x.Tools)
            .HasColumnName("tools")
            .HasColumnType("jsonb")
            .HasColumnOrder(13)
            .HasComment("Khai báo function/tool calling");
        builder.Property(x => x.TrafficPercent)
            .HasColumnName("traffic_percent")
            .HasColumnType("smallint")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("100")
            .ValueGeneratedNever()
            .HasComment("% lưu lượng (A/B test giữa các phiên bản PUBLISHED)");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("'DRAFT'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.ChangeNote)
            .HasColumnName("change_note")
            .HasColumnType("text")
            .HasColumnOrder(16)
            .HasComment("Ghi chú thay đổi");
        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Phát hành lúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(21)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(22)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.TemplateId, x.VersionNo }, "uq_ai_prompt_versions").IsUnique();

        builder.HasOne(x => x.Template)
            .WithMany(p => p.AiPromptVersions)
            .HasForeignKey(x => x.TemplateId)
            .HasConstraintName("prompt_versions_template_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ModelAlias)
            .WithMany()
            .HasForeignKey(x => x.ModelAliasId)
            .HasConstraintName("prompt_versions_model_alias_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
