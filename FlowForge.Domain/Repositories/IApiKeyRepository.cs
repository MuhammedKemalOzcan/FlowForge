using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Repositories
{
    public interface IApiKeyRepository
    {
        Task<ApiKey?> GetByIdForTenant(Guid apiKeyId, Guid tenantId, CancellationToken cancellationToken);

        void Update(ApiKey apiKey);

        void Add(ApiKey apiKey);
    }
}