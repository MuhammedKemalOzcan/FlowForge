using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.OwnsOne(x => x.ExternalIdentityId, externalIdBuilder =>
            {
                externalIdBuilder.Property(e => e.ExternalId)
                .IsRequired();

                externalIdBuilder.HasIndex(x => x.ExternalId)
                .IsUnique();
            });

            builder.OwnsOne(x => x.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value)
                .HasMaxLength(255)
                .IsRequired();
            });
        }
    }
}