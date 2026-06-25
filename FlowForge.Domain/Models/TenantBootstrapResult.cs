using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Models
{
    public record TenantBootstrapResult(Tenant Tenant, User AnonymousUser);
}