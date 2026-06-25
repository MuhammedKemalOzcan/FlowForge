namespace FlowForge.Application.Dtos
{
    public class DemoBootstrapResultDto
    {
        public Guid TenantId { get; set; }
        public string ApiKey { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
