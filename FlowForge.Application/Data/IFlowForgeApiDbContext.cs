using FlowForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Application.Data
{
    public interface IFlowForgeApiDbContext
    {
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }
        public DbSet<WebhookEndpoint> WebhookEndpoints { get; set; }
    }
}