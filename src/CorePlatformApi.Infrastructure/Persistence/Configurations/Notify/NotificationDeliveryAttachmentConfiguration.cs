// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho <c>notify.delivery_attachments</c>.
/// </summary>
public sealed class NotificationDeliveryAttachmentConfiguration : IEntityTypeConfiguration<NotificationDeliveryAttachment>
{
    public void Configure(EntityTypeBuilder<NotificationDeliveryAttachment> builder)
    {
        builder.ToTable("delivery_attachments", "notify", tb =>
        {
            tb.HasComment("File đính kèm email");
        });

        builder.HasKey(x => new { x.DeliveryId, x.FileId }).HasName("delivery_attachments_pkey");

        builder.Property(x => x.DeliveryId)
            .HasColumnName("delivery_id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Lượt gửi");
        builder.Property(x => x.FileId)
            .HasColumnName("file_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("File đính kèm");


        builder.HasOne(x => x.Delivery)
            .WithMany(p => p.NotificationDeliveryAttachments)
            .HasForeignKey(x => x.DeliveryId)
            .HasConstraintName("delivery_attachments_delivery_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("delivery_attachments_file_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
