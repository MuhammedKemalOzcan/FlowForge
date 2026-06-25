using FlowForge.Application.Abstractions;
using FlowForge.Application.Streaming;
using FlowForge.Domain.Entities;
using FlowForge.Domain.Repositories;

namespace FlowForge.API.BackgroundServices
{
    public class DeliveryRecoveryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeliveryRecoveryWorker> _logger;

        private static readonly TimeSpan WorkerInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan QueuedTimeout = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan InProgressTimeout = TimeSpan.FromMinutes(5);

        public DeliveryRecoveryWorker(IServiceScopeFactory scopeFactory, ILogger<DeliveryRecoveryWorker> logger)
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
                    await RecoverStuckDeliveriesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Uygulama kapanırken Task.Delay, DB call veya başka async işlem cancellation fırlatabilir.
                    // Bu gerçek bir hata değildir. Worker düzgün şekilde kapanıyor demektir.
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Delivery recovery cycle failed");
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

        private async Task RecoverStuckDeliveriesAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                _logger.LogInformation("Recovery cycle Started.");
                var repo = scope.ServiceProvider.GetRequiredService<IWebhookDeliveryRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var streamBroadcaster = scope.ServiceProvider.GetRequiredService<IWebhookDeliveryStreamBroadcaster>();

                var now = DateTime.UtcNow;
                var queuedThreshold = now - QueuedTimeout;
                var inProgressThreshold = now - InProgressTimeout;

                var queuedDeliveries = await repo.GetQueuedStuckDeliveriesAsync(queuedThreshold, cancellationToken);
                var inProgressDeliveries = await repo.GetInProgressStuckDeliveriesAsync(inProgressThreshold, cancellationToken);

                _logger.LogInformation("Found {count} queued deliveries.", queuedDeliveries.Count);
                _logger.LogInformation("Found {count} in progress deliveries.", inProgressDeliveries.Count);

                var hasChanges = false;
                var recovered = new List<WebhookDelivery>();

                foreach (var queued in queuedDeliveries)
                {
                    try
                    {
                        queued.RecoverStuckToPending();
                        hasChanges = true;
                        recovered.Add(queued);
                        _logger.LogInformation(
                        "Recovered stuck delivery {DeliveryId} from Queued to Pending",
                        queued.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to stuck delivery {DeliveryId} from {StuckState}.",
                            queued.Id,
                            queued.Status);
                    }
                }
                foreach (var inProgress in inProgressDeliveries)
                {
                    try
                    {
                        inProgress.RecoverStuckToPending();
                        hasChanges = true;
                        recovered.Add(inProgress);
                        _logger.LogInformation(
                        "Recovered stuck delivery {DeliveryId} from Queued to Pending",
                        inProgress.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to stuck delivery {DeliveryId} from {StuckState}.",
                            inProgress.Id,
                            inProgress.Status);
                    }
                }

                _logger.LogInformation("recover cycle completed.");
                if (hasChanges)
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    foreach (var delivery in recovered)
                    {
                        await streamBroadcaster.PublishAsync(WebhookDeliveryStreamEvent.From(delivery, "recovered"), cancellationToken);
                    }
                }
            }
        }
    }
}