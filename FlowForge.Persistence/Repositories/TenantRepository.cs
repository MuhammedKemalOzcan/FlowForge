using FlowForge.Domain.Entities;
using FlowForge.Domain.Repositories;
using FlowForge.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Persistence.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly FlowForgeAPIDbContext _context;

        public TenantRepository(FlowForgeAPIDbContext context)
        {
            _context = context;
        }

        public void Add(Tenant tenant)
        {
            _context.Add(tenant);
        }

        public async Task<Tenant?> GetById(Guid tenantId)
        {
            return await _context.Tenants
                 .Include(x => x.Memberships)
                 .FirstOrDefaultAsync(x => x.Id == tenantId);
        }

        public void Remove(Tenant tenant)
        {
            _context.Remove(tenant);
        }

        public void Update(Tenant tenant)
        {
            _context.Update(tenant);
        }
    }
}