using FlowForge.Domain.Entities;
using FlowForge.Domain.Enums;
using FlowForge.Domain.Repositories;
using FlowForge.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Persistence.Repositories
{
    public class WebhookDeliveryRepository : IWebhookDeliveryRepository
    {
        private readonly FlowForgeAPIDbContext _context;

        public WebhookDeliveryRepository(FlowForgeAPIDbContext context)
        {
            _context = context;
        }

        public void Add(WebhookDelivery delivery)
        {
            _context.Add(delivery);
        }

        public async Task<WebhookDelivery?> GetByIdAsync(Guid id, Guid tenantId)
        {
            return await _context.WebhookDeliveries
                .Include(x => x.Attempts)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        }

        public async Task<WebhookDelivery?> GetByIdempotencyKey(string idempotencyKey, Guid tenantId)
        {
            return await _context.WebhookDeliveries
                .Include(x => x.Attempts)
                .FirstOrDefaultAsync(x => x.IdempotencyKey.Value == idempotencyKey && x.TenantId == tenantId);
        }

        public async Task<List<WebhookDelivery>> GetPendingDeliveriesAsync()
        {
            return await _context.WebhookDeliveries
                .AsNoTracking()
                .Where(x => x.Status == DeliveryStatus.Pending && (x.NextRetryAt == null || x.NextRetryAt <= DateTime.UtcNow))
                .Take(50)
                .ToListAsync();
        }

        public void Remove(WebhookDelivery delivery)
        {
            _context.Remove(delivery);
        }

        public void Update(WebhookDelivery delivery)
        {
            _context.Update(delivery);
        }
    }
}