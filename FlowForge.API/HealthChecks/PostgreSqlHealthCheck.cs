using FlowForge.Persistence.Contexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowForge.API.HealthChecks;

public class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PostgreSqlHealthCheck(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<FlowForgeAPIDbContext>();
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("PostgreSQL unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}
