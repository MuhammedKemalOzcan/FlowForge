using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.HasKey("TenantId", nameof(Membership.UserId));

            builder.Property(x => x.Role)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.JoinedAt)
                .IsRequired();
        }
    }
}