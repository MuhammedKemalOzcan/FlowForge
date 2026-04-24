using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetById(Guid tenantId);

        void Add(Tenant tenant);

        void Update(Tenant tenant);

        void Remove(Tenant tenant);
    }
}