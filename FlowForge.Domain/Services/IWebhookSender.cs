using FlowForge.Domain.Models;

namespace FlowForge.Domain.Services
{
    public interface IWebhookSender
    {
        Task<WebhookSendResult> SendAsync(string url, string payload, string signingSecret, string eventType, Guid deliveryId, CancellationToken cancellationToken);
    }
}