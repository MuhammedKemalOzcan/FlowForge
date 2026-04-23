using FlowForge.Domain.Entities;
using FlowForge.Domain.Repositories;
using FlowForge.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Persistence.Repositories
{
    public class WebhookEndpointRepository : IWebhookEndpointRepository
    {
        private readonly FlowForgeAPIDbContext _context;

        public WebhookEndpointRepository(FlowForgeAPIDbContext context)
        {
            _context = context;
        }

        public void Add(WebhookEndpoint endpoint)
        {
            _context.WebhookEndpoints.Add(endpoint);
        }

        public async Task<List<WebhookEndpoint>> GetAllAsync(Guid tenantId)
        {
            return await _context.WebhookEndpoints
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<WebhookEndpoint?> GetByIdAsync(Guid id, Guid tenantId)
        {
            return await _context.WebhookEndpoints
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id);
        }

        public void Remove(WebhookEndpoint endpoint)
        {
            _context.WebhookEndpoints.Remove(endpoint);
        }

        public void Update(WebhookEndpoint endpoint)
        {
            _context.WebhookEndpoints.Update(endpoint);
        }
    }
}