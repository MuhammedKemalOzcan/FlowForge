using System.Linq;
using FlowForge.Domain.Entities;

namespace FlowForge.Application.Streaming
{
    /// <summary>
    /// A single immutable webhook-delivery lifecycle event broadcast to connected SSE clients.
    /// </summary>
    public record WebhookDeliveryStreamEvent
    {
        public string EventName { get; init; } = default!;
        public Guid DeliveryId { get; init; }
        public Guid TenantId { get; init; }
        public Guid EndpointId { get; init; }
        public string EventType { get; init; } = default!;
        public string Status { get; init; } = default!;
        public int? AttemptNumber { get; init; }
        public int? StatusCode { get; init; }
        public long? DurationMs { get; init; }
        public string? ErrorMessage { get; init; }
        public DateTime? NextRetryAt { get; init; }
        public DateTime OccurredAt { get; init; }

        /// <summary>
        /// Builds a stream event from the current state of a delivery, enriching it with the
        /// details of the most recent attempt when one exists.
        /// </summary>
        public static WebhookDeliveryStreamEvent From(WebhookDelivery delivery, string eventName)
        {
            var lastAttempt = delivery.Attempts.LastOrDefault();

            return new WebhookDeliveryStreamEvent
            {
                EventName = eventName,
                DeliveryId = delivery.Id,
                TenantId = delivery.TenantId,
                EndpointId = delivery.EndpointId,
                EventType = delivery.EventType.Value,
                Status = delivery.Status.ToString(),
                AttemptNumber = lastAttempt?.AttemptNumber,
                StatusCode = (int?)lastAttempt?.StatusCode,
                DurationMs = lastAttempt?.DurationMs,
                ErrorMessage = lastAttempt?.ErrorMessage,
                NextRetryAt = delivery.NextRetryAt,
                OccurredAt = DateTime.UtcNow
            };
        }
    }
}
