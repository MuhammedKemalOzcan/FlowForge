using FlowForge.Domain.Enums;

namespace FlowForge.Application.Dtos
{
    public class WebhookDeliveryDto
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DeliveryStatus Status { get; set; }
        public RetryPolicyDto RetryPolicy { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public DateTime? FinalResultAt { get; set; }
        public List<DeliveryAttemptDto> DeliveryAttempts { get; set; }
    }
}