using FlowForge.Application.Messages;
using FlowForge.Domain.Repositories;
using MassTransit;

namespace FlowForge.API.BackgroundServices
{
    public class DeliveryProcessorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeliveryProcessorWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope()) //Yeni scope
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IWebhookDeliveryRepository>();
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                    var deliveries = await repo.GetPendingDeliveriesAsync();

                    foreach (var delivery in deliveries)
                    {
                        try
                        {
                            await publishEndpoint.Publish(
                                new ProcessWebhookDeliveryMessage(delivery.Id, delivery.TenantId), stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            // Bu delivery patladı ama diğerleri etkilenmemeli
                            Console.WriteLine($"Delivery {delivery.Id} processing failed: {ex.Message}");
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}