using FlowForge.Domain.ValueObjects;

namespace FlowForge.Domain.Entities
{
    public class WebhookEndpoint
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Url TargetUrl { get; private set; }
        public List<string> SubscribedEventTypes { get; private set; }
        public SigningSecret SigningSecret { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
    }
}