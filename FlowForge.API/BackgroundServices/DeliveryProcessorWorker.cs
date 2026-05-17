using FlowForge.Application.Messages;
using FlowForge.Domain.Repositories;
using MassTransit;

namespace FlowForge.API.BackgroundServices
{
    public class DeliveryProcessorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeliveryProcessorWorker> _logger;

        public DeliveryProcessorWorker(IServiceScopeFactory scopeFactory, ILogger<DeliveryProcessorWorker> logger)
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
                    using (var scope = _scopeFactory.CreateScope()) //Yeni scope
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<IWebhookDeliveryRepository>();
                        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var deliveries = await repo.GetPendingDeliveriesAsync(stoppingToken);

                        foreach (var delivery in deliveries)
                        {
                            try
                            {
                                // State stays Queued. DeliveryRecoveryWorker will recover after timeout.
                                delivery.MarkQueued();
                                await unitOfWork.SaveChangesAsync(stoppingToken);

                                await publishEndpoint.Publish(
                                    new ProcessWebhookDeliveryMessage(delivery.Id, delivery.TenantId), stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                // Bu delivery patladı ama diğerleri etkilenmemeli
                                _logger.LogError(ex, "Failed to queue delivery {DeliveryId} for tenant {TenantId}", delivery.Id, delivery.TenantId);
                            }
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Delivery Process cycle failed.");
                    try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }
    }
}