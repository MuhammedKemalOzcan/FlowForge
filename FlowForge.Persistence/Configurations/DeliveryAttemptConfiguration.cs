using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
    {
        public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AttemptNumber)
                .IsRequired();

            builder.Property(x => x.StartedAt).IsRequired();
            builder.Property(x => x.CompletedAt).IsRequired();

            builder.Property(x => x.DurationMs).IsRequired();

            builder.Property(x => x.StatusCode)
                .HasConversion<string>();

            builder.Property(x => x.Outcome)
                .HasConversion<string>()
                .IsRequired();
        }
    }
}