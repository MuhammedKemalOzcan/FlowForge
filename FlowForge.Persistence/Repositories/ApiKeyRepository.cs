using FlowForge.Domain.Entities;
using FlowForge.Domain.Repositories;
using FlowForge.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Persistence.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly FlowForgeAPIDbContext _context;

        public ApiKeyRepository(FlowForgeAPIDbContext context)
        {
            _context = context;
        }

        public void Add(ApiKey apiKey)
        {
            _context.ApiKeys
                .Add(apiKey);
        }

        public Task<ApiKey?> GetByIdForTenant(Guid apiKeyId, Guid tenantId, CancellationToken cancellationToken)
        {
            var apiKey = _context.ApiKeys
                .FirstOrDefaultAsync(x => x.Id == apiKeyId && x.TenantId == tenantId, cancellationToken);

            return apiKey;
        }

        public void Update(ApiKey apiKey)
        {
            _context.ApiKeys
                .Update(apiKey);
        }
    }
}