// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho bảng <c>ai_providers</c>.
/// </summary>
public sealed class AiProviderConfiguration : IEntityTypeConfiguration<AiProvider>
{
    public void Configure(EntityTypeBuilder<AiProvider> builder)
    {
        builder.ToTable("ai_providers", tb =>
        {
            tb.HasComment("Nhà cung cấp AI (OpenAI, Gemini, Anthropic, Azure OpenAI, model nội bộ qua Ollama/vLLM)");
            tb.HasCheckConstraint("ai_providers_api_style_check", "api_style IN ('OPENAI','ANTHROPIC','GEMINI','CUSTOM')");
        });

        builder.HasKey(x => x.Id).HasName("ai_providers_pkey");

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
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã (OPENAI, GEMINI, ANTHROPIC, AZURE_OPENAI, OLLAMA, VLLM)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên");
        builder.Property(x => x.ApiStyle)
            .HasColumnName("api_style")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Chuẩn API để chọn adapter");
        builder.Property(x => x.BaseUrl)
            .HasColumnName("base_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .HasComment("Base URL mặc định");
        builder.Property(x => x.IsSelfHosted)
            .HasColumnName("is_self_hosted")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Model nội bộ (dữ liệu không rời hạ tầng)");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn dùng");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.Code, "ai_providers_code_key").IsUnique();
    }
}
