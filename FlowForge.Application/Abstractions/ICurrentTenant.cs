namespace FlowForge.Application.Abstractions
{
    public interface ICurrentTenant
    {
        Guid? TenantId { get; }
        Guid? ApiKeyId { get; }

        Guid GetRequiredTenantId();
    }
}