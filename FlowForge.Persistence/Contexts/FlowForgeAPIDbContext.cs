using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Persistence.Contexts
{
    public class FlowForgeAPIDbContext : DbContext
    {
        public FlowForgeAPIDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }
        public DbSet<WebhookEndpoint> WebhookEndpoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlowForgeAPIDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}