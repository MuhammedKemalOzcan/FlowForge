using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowForge.Persistence.Configurations
{
    public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
    {
        public void Configure(EntityTypeBuilder<ApiKey> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Prefix)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.OwnsOne(x => x.Key, keyBuilder =>
            {
                keyBuilder.Property(k => k.Value)
                .HasMaxLength(100)
                .IsRequired();

                keyBuilder.HasIndex(k => k.Value)
                .IsUnique();
            });
        }
    }
}