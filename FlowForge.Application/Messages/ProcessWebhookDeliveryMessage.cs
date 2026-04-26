namespace FlowForge.Application.Messages
{
    public record ProcessWebhookDeliveryMessage(Guid DeliveryId, Guid TenantId);
}