using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Repositories
{
    public interface IWebhookDeliveryRepository
    {
        Task<List<WebhookDelivery>> GetAllAsync(Guid tenantId);

        Task<WebhookDelivery?> GetByIdAsync(Guid Id, Guid tenantId);

        Task<WebhookDelivery?> GetByIdempotencyKey(string idempotencyKey, Guid tenantId);

        void Add(WebhookDelivery delivery);

        void Update(WebhookDelivery delivery);

        void Remove(WebhookDelivery delivery);
    }
}