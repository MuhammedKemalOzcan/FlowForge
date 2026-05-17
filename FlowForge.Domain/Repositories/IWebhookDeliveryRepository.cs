using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Repositories
{
    public interface IWebhookDeliveryRepository
    {
        Task<List<WebhookDelivery>> GetPendingDeliveriesAsync(CancellationToken cancellationToken);

        Task<List<WebhookDelivery>> GetQueuedStuckDeliveriesAsync(DateTime threshold, CancellationToken cancellationToken);

        Task<List<WebhookDelivery>> GetInProgressStuckDeliveriesAsync(DateTime threshold, CancellationToken cancellationToken);

        Task<WebhookDelivery?> GetByIdAsync(Guid Id, Guid tenantId,CancellationToken cancellationToken = default);

        Task<WebhookDelivery?> GetByIdempotencyKey(string idempotencyKey, Guid tenantId);

        void Add(WebhookDelivery delivery);

        void Update(WebhookDelivery delivery);

        void Remove(WebhookDelivery delivery);
    }
}