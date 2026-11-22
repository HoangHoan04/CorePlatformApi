// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Ai;

/// <summary>
/// Cấu hình EF Core cho <c>ai.usage_logs</c>.
/// </summary>
public sealed class AiUsageLogConfiguration : IEntityTypeConfiguration<AiUsageLog>
{
    public void Configure(EntityTypeBuilder<AiUsageLog> builder)
    {
        builder.ToTable("usage_logs", "ai", tb =>
        {
            tb.HasComment("Log từng lần gọi AI: token, chi phí, độ trễ (không lưu nội dung prompt theo mặc định vì dữ liệu nhạy cảm)");
            tb.HasCheckConstraint("usage_logs_request_type_check", "request_type IN ('CHAT','EMBEDDING','RERANK','IMAGE','AUDIO','MODERATION')");
            tb.HasCheckConstraint("usage_logs_status_check", "status IN ('SUCCESS','ERROR','BLOCKED','QUOTA_EXCEEDED','TIMEOUT')");
            tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql
        });

        builder.HasKey(x => new { x.Id, x.CreatedAt }).HasName("usage_logs_pkey");

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
            .HasComment("Tenant");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(2)
            .HasComment("Phân hệ gọi");
        builder.Property(x => x.GatewayClientId)
            .HasColumnName("gateway_client_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Client nội bộ");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("User cuối");
        builder.Property(x => x.PromptTemplateId)
            .HasColumnName("prompt_template_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Prompt");
        builder.Property(x => x.PromptVersionId)
            .HasColumnName("prompt_version_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Phiên bản prompt");
        builder.Property(x => x.AliasCode)
            .HasColumnName("alias_code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(7)
            .HasComment("Alias được yêu cầu");
        builder.Property(x => x.ModelId)
            .HasColumnName("model_id")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Model thực tế phục vụ");
        builder.Property(x => x.CredentialId)
            .HasColumnName("credential_id")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Key provider đã dùng");
        builder.Property(x => x.RequestType)
            .HasColumnName("request_type")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'CHAT'")
            .ValueGeneratedNever()
            .HasComment("Loại request");
        builder.Property(x => x.InputTokens)
            .HasColumnName("input_tokens")
            .HasColumnType("int")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Token đầu vào");
        builder.Property(x => x.OutputTokens)
            .HasColumnName("output_tokens")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Token đầu ra");
        builder.Property(x => x.CachedTokens)
            .HasColumnName("cached_tokens")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Token đọc từ cache");
        builder.Property(x => x.TotalTokens)
            .HasColumnName("total_tokens")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .HasComputedColumnSql("input_tokens + output_tokens", stored: true)
            .HasComment("Tổng token");
        builder.Property(x => x.EstimatedCostUsd)
            .HasColumnName("estimated_cost_usd")
            .HasColumnType("numeric(14,8)")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Chi phí ước tính (USD)");
        builder.Property(x => x.LatencyMs)
            .HasColumnName("latency_ms")
            .HasColumnType("int")
            .HasColumnOrder(16)
            .HasComment("Độ trễ tổng");
        builder.Property(x => x.TimeToFirstTokenMs)
            .HasColumnName("time_to_first_token_ms")
            .HasColumnType("int")
            .HasColumnOrder(17)
            .HasComment("Thời gian tới token đầu (streaming)");
        builder.Property(x => x.IsFallback)
            .HasColumnName("is_fallback")
            .HasColumnType("boolean")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Phục vụ bởi model fallback");
        builder.Property(x => x.IsCacheHit)
            .HasColumnName("is_cache_hit")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Trả về từ semantic cache");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(20)
            .IsRequired()
            .HasComment("Kết quả");
        builder.Property(x => x.ErrorCode)
            .HasColumnName("error_code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(21)
            .HasComment("Mã lỗi");
        builder.Property(x => x.GuardrailCodes)
            .HasColumnName("guardrail_codes")
            .HasColumnType("text[]")
            .HasColumnOrder(22)
            .HasComment("Guardrail đã kích hoạt");
        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(23)
            .HasComment("Trace id");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm gọi");

    }
}
