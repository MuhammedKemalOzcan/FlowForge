namespace FlowForge.Application.Dtos
{
    public class WebhookEndpointDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TargetUrl { get; set; }
        public List<string> EventTypes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public RetryPolicyDto RetryPolicy { get; set; }
    }
}