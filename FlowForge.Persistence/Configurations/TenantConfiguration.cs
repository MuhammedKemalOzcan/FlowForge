using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Plan)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.TenantStatus)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasMany(x => x.Memberships)
                .WithOne()
                .HasForeignKey("TenantId"); //shadow property -> Db'de kolon var ama entity içerisinde yok

            builder.OwnsOne(x => x.PlanLimits, planBuilder =>
            {
                planBuilder.Property(x => x.MaxEndpointsAllowed)
                .IsRequired();

                planBuilder.Property(x => x.MaxEventsPerMinute)
                .IsRequired();

                planBuilder.Property(x => x.MaxMembersAllowed)
                .IsRequired();
            });
        }
    }
}