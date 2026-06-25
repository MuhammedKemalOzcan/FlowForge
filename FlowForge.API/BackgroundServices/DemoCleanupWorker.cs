using FlowForge.Application.Data;
using FlowForge.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.API.BackgroundServices
{
    public class DemoCleanupWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DemoCleanupWorker> _logger;

        private static readonly TimeSpan WorkerInterval = TimeSpan.FromHours(1);

        public DemoCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<DemoCleanupWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredDemosAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Demo cleanup cycle failed");
                }
                try
                {
                    await Task.Delay(WorkerInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task CleanupExpiredDemosAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IFlowForgeApiDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredTenants = await tenantRepo.GetExpiredDemoTenantsAsync(DateTime.UtcNow, cancellationToken);

            if (expiredTenants.Count == 0)
            {
                _logger.LogInformation("No expired demo tenants found.");
                return;
            }

            _logger.LogInformation("Found {Count} expired demo tenants to clean up.", expiredTenants.Count);

            foreach (var tenant in expiredTenants)
            {
                try
                {
                    var tenantId = tenant.Id;
                    var ownerUserId = tenant.Memberships.FirstOrDefault()?.UserId;

                    await dbContext.ApiKeys
                        .Where(k => k.TenantId == tenantId)
                        .ExecuteDeleteAsync(cancellationToken);

                    await dbContext.WebhookDeliveries
                        .Where(d => d.TenantId == tenantId)
                        .ExecuteDeleteAsync(cancellationToken);

                    await dbContext.WebhookEndpoints
                        .Where(e => e.TenantId == tenantId)
                        .ExecuteDeleteAsync(cancellationToken);

                    tenantRepo.Remove(tenant);
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    if (ownerUserId.HasValue)
                    {
                        var user = await userRepo.GetByIdAsync(ownerUserId.Value);
                        if (user != null)
                        {
                            userRepo.Remove(user);
                            await unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                    }

                    _logger.LogInformation("Deleted expired demo tenant {TenantId}.", tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete demo tenant {TenantId}.", tenant.Id);
                }
            }
        }
    }
}
