using System.Collections.Concurrent;
using System.Threading.Channels;
using FlowForge.Application.Abstractions;
using FlowForge.Application.Streaming;
using Microsoft.Extensions.Logging;

namespace FlowForge.Infrastructure.Streaming
{
    public class InMemoryWebhookDeliveryStreamBroadcaster : IWebhookDeliveryStreamBroadcaster
    {
        private const int SubscriberCapacity = 100;

        private readonly ConcurrentDictionary<Guid, Subscriber> _subscribers = new();
        private readonly ILogger<InMemoryWebhookDeliveryStreamBroadcaster> _logger;

        public InMemoryWebhookDeliveryStreamBroadcaster(ILogger<InMemoryWebhookDeliveryStreamBroadcaster> logger)
        {
            _logger = logger;
        }

        public ValueTask PublishAsync(WebhookDeliveryStreamEvent evt, CancellationToken ct = default)
        {
            foreach (var subscriber in _subscribers.Values)
            {
                if (!subscriber.Matches(evt))
                    continue;

                // Bounded channel with DropOldest — TryWrite never fails or blocks.
                subscriber.Channel.Writer.TryWrite(evt);
            }

            return ValueTask.CompletedTask;
        }

        public IWebhookDeliveryStreamSubscription Subscribe(Guid tenantId, Guid? endpointId = null, Guid? deliveryId = null)
        {
            var channel = Channel.CreateBounded<WebhookDeliveryStreamEvent>(new BoundedChannelOptions(SubscriberCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

            var id = Guid.NewGuid();
            var subscriber = new Subscriber(tenantId, endpointId, deliveryId, channel);
            _subscribers[id] = subscriber;

            _logger.LogDebug("SSE subscriber {SubscriberId} connected for tenant {TenantId} (endpoint: {EndpointId}, delivery: {DeliveryId}). Active: {Count}",
                id, tenantId, endpointId, deliveryId, _subscribers.Count);

            return new Subscription(this, id, channel.Reader);
        }

        private void Unsubscribe(Guid id)
        {
            if (_subscribers.TryRemove(id, out var subscriber))
            {
                subscriber.Channel.Writer.TryComplete();
                _logger.LogDebug("SSE subscriber {SubscriberId} disconnected. Active: {Count}", id, _subscribers.Count);
            }
        }

        private sealed class Subscriber
        {
            private readonly Guid _tenantId;
            private readonly Guid? _endpointId;
            private readonly Guid? _deliveryId;

            public Channel<WebhookDeliveryStreamEvent> Channel { get; }

            public Subscriber(Guid tenantId, Guid? endpointId, Guid? deliveryId, Channel<WebhookDeliveryStreamEvent> channel)
            {
                _tenantId = tenantId;
                _endpointId = endpointId;
                _deliveryId = deliveryId;
                Channel = channel;
            }

            public bool Matches(WebhookDeliveryStreamEvent evt)
            {
                if (evt.TenantId != _tenantId) return false;
                if (_endpointId.HasValue && evt.EndpointId != _endpointId.Value) return false;
                if (_deliveryId.HasValue && evt.DeliveryId != _deliveryId.Value) return false;
                return true;
            }
        }

        private sealed class Subscription : IWebhookDeliveryStreamSubscription
        {
            private readonly InMemoryWebhookDeliveryStreamBroadcaster _owner;
            private readonly Guid _id;

            public ChannelReader<WebhookDeliveryStreamEvent> Reader { get; }

            public Subscription(InMemoryWebhookDeliveryStreamBroadcaster owner, Guid id, ChannelReader<WebhookDeliveryStreamEvent> reader)
            {
                _owner = owner;
                _id = id;
                Reader = reader;
            }

            public void Dispose() => _owner.Unsubscribe(_id);
        }
    }
}