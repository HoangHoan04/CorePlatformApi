// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho <c>notify.user_preferences</c>.
/// </summary>
public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("user_preferences", "notify", tb =>
        {
            tb.HasComment("Tùy chọn nhận thông báo của user theo sự kiện × kênh, giờ yên lặng");
            tb.HasCheckConstraint("user_preferences_channel_check", "channel IN ('IN_APP','PUSH','EMAIL','SMS','ZALO')");
            tb.HasCheckConstraint("user_preferences_digest_mode_check", "digest_mode IN ('INSTANT','HOURLY','DAILY')");
        });

        builder.HasKey(x => x.Id).HasName("user_preferences_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.EventTypeId)
            .HasColumnName("event_type_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Sự kiện (NULL = áp dụng mọi sự kiện)");
        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Kênh");
        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bật/tắt");
        builder.Property(x => x.QuietStart)
            .HasColumnName("quiet_start")
            .HasColumnType("time")
            .HasColumnOrder(5)
            .HasComment("Bắt đầu giờ yên lặng (không push)");
        builder.Property(x => x.QuietEnd)
            .HasColumnName("quiet_end")
            .HasColumnType("time")
            .HasColumnOrder(6)
            .HasComment("Kết thúc giờ yên lặng");
        builder.Property(x => x.DigestMode)
            .HasColumnName("digest_mode")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'INSTANT'")
            .ValueGeneratedNever()
            .HasComment("Gửi ngay hay gom tóm tắt");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Cập nhật lúc");

        builder.HasIndex(x => new { x.UserId, x.EventTypeId, x.Channel }, "uq_user_notify_prefs")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserNotificationPreferences)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_preferences_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.EventType)
            .WithMany(p => p.UserNotificationPreferences)
            .HasForeignKey(x => x.EventTypeId)
            .HasConstraintName("user_preferences_event_type_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
