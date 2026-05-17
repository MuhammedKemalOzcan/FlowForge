using FlowForge.Domain.Enums;
using FlowForge.Domain.ValueObjects;
using System.Net;

namespace FlowForge.Domain.Entities
{
    public class WebhookDelivery
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid EndpointId { get; private set; }
        public EventType EventType { get; private set; }
        public string Payload { get; private set; }
        public IdempotencyKey IdempotencyKey { get; private set; }
        public DeliveryStatus Status { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }
        public DateTime ReceivedAt { get; private set; }
        public DateTime? NextRetryAt { get; private set; }
        public DateTime? FinalResultAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private List<DeliveryAttempt> _attempts = new();
        public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

        private WebhookDelivery()
        { }

        public static WebhookDelivery Create(Guid tenantId, Guid endpointId, EventType eventType, string payload, IdempotencyKey idempotencyKey, RetryPolicy retryPolicy)
        {
            if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id cannot be empty", nameof(tenantId));
            if (endpointId == Guid.Empty) throw new ArgumentException("Endpoint id cannot be empty", nameof(endpointId));
            if (eventType is null) throw new ArgumentNullException(nameof(eventType));
            if (string.IsNullOrEmpty(payload)) throw new ArgumentException("Payload cannot be null");
            if (idempotencyKey is null) throw new ArgumentException(nameof(idempotencyKey));
            if (retryPolicy is null) throw new ArgumentException(nameof(retryPolicy));

            var webhookDelivery = new WebhookDelivery()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EndpointId = endpointId,
                EventType = eventType,
                Payload = payload,
                IdempotencyKey = idempotencyKey,
                Status = DeliveryStatus.Pending,
                RetryPolicy = retryPolicy,
                ReceivedAt = DateTime.UtcNow,
                NextRetryAt = null,
                FinalResultAt = null,
            };

            return webhookDelivery;
        }

        public void RecordSuccessfulAttempt(long durationMs, HttpStatusCode? statusCode, string? responseBodySnippet, DateTime startedAt, DateTime completedAt)
        {
            if (Status != DeliveryStatus.InProgress) throw new InvalidOperationException($"Cannot mark as Succeeded from {Status} state. Only InProgress is allowed.");

            var deliveryAttempt = new DeliveryAttempt(_attempts.Count + 1, durationMs, statusCode, responseBodySnippet, null, startedAt, completedAt, OutcomeStatus.Succeeded);

            _attempts.Add(deliveryAttempt);

            MarkAsSucceeded();
        }

        public void RecordFailedAttempt(long durationMs, HttpStatusCode? statusCode, string? errorMessage, DateTime startedAt, DateTime completedAt)
        {
            if (Status != DeliveryStatus.InProgress) throw new InvalidOperationException($"Cannot mark as Failed from {Status} state. Only InProgress is allowed.");

            var attemptCount = _attempts.Count + 1;

            if (RetryPolicy.IsLastAttempt(attemptCount))
            {
                var deliveryAttempt = new DeliveryAttempt(_attempts.Count + 1, durationMs, statusCode, null, errorMessage, startedAt, completedAt, OutcomeStatus.FailedFinal);

                _attempts.Add(deliveryAttempt);

                MarkDeadLettered();
            }
            else
            {
                var deliveryAttempt = new DeliveryAttempt(_attempts.Count + 1, durationMs, statusCode, null, errorMessage, startedAt, completedAt, OutcomeStatus.FailedWillRetry);

                _attempts.Add(deliveryAttempt);

                Status = DeliveryStatus.Pending;
                FinalResultAt = null;
                NextRetryAt = DateTime.UtcNow + RetryPolicy.CalculateDelayFor(attemptCount);
            }
        }

        public void MarkInProgress()
        {
            if (Status != DeliveryStatus.Queued)
                throw new InvalidOperationException($"Cannot mark as InProgress from {Status} state.");

            Status = DeliveryStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkQueued()
        {
            if (Status != DeliveryStatus.Pending)
                throw new InvalidOperationException($"Cannot mark as Queued from {Status} state.");

            Status = DeliveryStatus.Queued;
            UpdatedAt = DateTime.UtcNow;
        }

        private void MarkDeadLettered()
        {
            if (Status != DeliveryStatus.InProgress) throw new InvalidOperationException($"Cannot mark as Dead lettered from {Status} state.");

            Status = DeliveryStatus.DeadLettered;
            FinalResultAt = DateTime.UtcNow;
            NextRetryAt = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecoverStuckToPending()
        {
            if (Status != DeliveryStatus.Queued && Status != DeliveryStatus.InProgress)
                throw new InvalidOperationException($"Cannot recover as pending from {Status} state.");

            Status = DeliveryStatus.Pending;
            UpdatedAt = DateTime.UtcNow;
        }

        private void MarkAsSucceeded()
        {
            if (Status == DeliveryStatus.InProgress)
            {
                Status = DeliveryStatus.Succeeded;
                FinalResultAt = DateTime.UtcNow;
                NextRetryAt = null;
                UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException($"Cannot mark as Succeeded from {Status} state. Only InProgress is allowed.");
            }
        }
    }
}