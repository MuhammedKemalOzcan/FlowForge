using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Repositories
{
    public interface IWebhookEndpointRepository
    {
        Task<WebhookEndpoint?> GetByIdAsync(Guid id, Guid tenantId);

        Task<List<WebhookEndpoint>> GetAllAsync(Guid tenantId);

        void Add(WebhookEndpoint endpoint);

        void Update(WebhookEndpoint endpoint);

        void Remove(WebhookEndpoint endpoint);
    }
}