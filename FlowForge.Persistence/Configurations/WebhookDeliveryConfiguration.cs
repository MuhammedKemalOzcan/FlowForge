using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
    {
        public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Payload)
                .IsRequired()
                .HasMaxLength(50000);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.ReceivedAt)
                .IsRequired();

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId);

            builder.HasOne<WebhookEndpoint>()
                .WithMany()
                .HasForeignKey(x => x.EndpointId);

            builder.HasMany<DeliveryAttempt>()
                .WithOne()
                .HasForeignKey("WebhookDeliveryId");

            builder.OwnsOne(x => x.EventType, eventTypeBuilder =>
            {
                eventTypeBuilder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(100);
            });
            builder.OwnsOne(x => x.IdempotencyKey, idempotencyKeyBuilder =>
            {
                idempotencyKeyBuilder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(255);

                idempotencyKeyBuilder.HasIndex(x => x.Value)
                .IsUnique();
            });

            builder.OwnsOne(x => x.RetryPolicy, policyBuilder =>
            {
                policyBuilder.Property(x => x.MaxAttempts)
                .IsRequired();

                policyBuilder.Property(x => x.Strategy)
                .IsRequired()
                .HasConversion<string>();

                policyBuilder.Property(x => x.InitialDelay)
                .IsRequired();

                policyBuilder.Property(x => x.MaxDelay)
                .IsRequired();

                policyBuilder.Property(x => x.TimeOut)
                .IsRequired();
            });
        }
    }
}