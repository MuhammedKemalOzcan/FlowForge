namespace FlowForge.Domain.Services
{
    public interface IRateLimiter
    {
        Task<bool> IsAllowedAsync(Guid tenantId, int MaxRequestsPerMinute);
    }
}