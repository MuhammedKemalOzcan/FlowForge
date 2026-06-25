using System.Threading.Channels;
using FlowForge.Application.Streaming;

namespace FlowForge.Application.Abstractions
{
    public interface IWebhookDeliveryStreamBroadcaster
    {
        ValueTask PublishAsync(WebhookDeliveryStreamEvent evt, CancellationToken ct = default);

        IWebhookDeliveryStreamSubscription Subscribe(Guid tenantId, Guid? endpointId = null, Guid? deliveryId = null);
    }

    public interface IWebhookDeliveryStreamSubscription : IDisposable
    {
        ChannelReader<WebhookDeliveryStreamEvent> Reader { get; }
    }
}