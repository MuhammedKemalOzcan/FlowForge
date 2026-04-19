using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class WebhookEndpointConfiguration : IEntityTypeConfiguration<WebhookEndpoint>
    {
        public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId);

            builder.OwnsOne(x => x.Name, nameBuilder =>
            {
                nameBuilder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(100);
            });
            builder.OwnsOne(x => x.TargetUrl, urlBuilder =>
            {
                urlBuilder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(255);
            });
            builder.OwnsOne(x => x.SigningSecret, secretBuilder =>
            {
                // Value property'si internal olduğu için string-based mapping
                secretBuilder.Property<string>("Value")
                .HasMaxLength(255);
            });
            builder.OwnsMany(x => x.SubscribedEventTypes, eventTypeBuilder =>
            {
                eventTypeBuilder.WithOwner().HasForeignKey("WebhookEndpointId"); //farklı tablolard abağlantı kursun diye
                eventTypeBuilder.Property(x => x.Value)
                .IsRequired();
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