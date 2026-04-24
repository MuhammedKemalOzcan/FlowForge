using FlowForge.Domain.ValueObjects;

namespace FlowForge.Domain.Entities
{
    public class WebhookEndpoint
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public EndpointName Name { get; private set; }
        public Url TargetUrl { get; private set; }
        private readonly List<EventType> _eventTypes = new();
        public IReadOnlyCollection<EventType> SubscribedEventTypes => _eventTypes.AsReadOnly();
        public SigningSecret SigningSecret { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private WebhookEndpoint()
        { }

        public static WebhookEndpoint Create(Guid tenantId, EndpointName name, Url targetUrl, List<EventType> subscribedEventTypes, SigningSecret signingSecret, RetryPolicy retryPolicy)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant cannot be found");
            if (name is null)
                throw new ArgumentException("Name cannot be null", nameof(name));
            if (targetUrl is null)
                throw new ArgumentException("Target URL cannot be null", nameof(targetUrl));
            if (subscribedEventTypes is null || !subscribedEventTypes.Any())
                throw new ArgumentException("At least one event type must be subscribed to", nameof(subscribedEventTypes));
            if (signingSecret is null)
                throw new ArgumentException("Signing secret cannot be null", nameof(signingSecret));
            if (retryPolicy is null)
                throw new ArgumentException("Retry policy cannot be null", nameof(retryPolicy));

            var endpoint = new WebhookEndpoint
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                TargetUrl = targetUrl,
                SigningSecret = signingSecret,
                RetryPolicy = retryPolicy,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            endpoint._eventTypes.AddRange(subscribedEventTypes);

            return endpoint;
        }

        public void ChangeRetryPolicy(RetryPolicy newPolicy)
        {
            if (newPolicy is null)
                throw new ArgumentException("Retry policy cannot be null", nameof(newPolicy));
            RetryPolicy = newPolicy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSubscription(IEnumerable<EventType> eventTypes)
        {
            if (eventTypes is null || !eventTypes.Any())
                throw new ArgumentException("At least one event type must be subscribed to", nameof(eventTypes));

            var toAdd = eventTypes.Except(_eventTypes).ToList();
            var toRemove = _eventTypes.Except(eventTypes).ToList();

            foreach (var eventType in toAdd) Subscribe(eventType);
            foreach (var eventType in toRemove) Unsubscribe(eventType);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Subscribe(EventType eventType)
        {
            if (_eventTypes.Contains(eventType)) return;
            _eventTypes.Add(eventType);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Unsubscribe(EventType eventType)
        {
            if (!_eventTypes.Contains(eventType)) return;
            if (_eventTypes.Count == 1)
                throw new InvalidOperationException("Cannot unsubscribe last event type");

            _eventTypes.Remove(eventType);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeTargetUrl(Url newUrl)
        {
            if (newUrl is null)
                throw new ArgumentException("Target URL cannot be null", nameof(newUrl));
            TargetUrl = newUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeName(EndpointName newName)
        {
            if (newName is null)
                throw new ArgumentException("Name cannot be null", nameof(newName));
            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        //idempotent davranarak zaten deactive durumdaysa hata fırlatmak yerine metottan sessizce çıktık
        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}