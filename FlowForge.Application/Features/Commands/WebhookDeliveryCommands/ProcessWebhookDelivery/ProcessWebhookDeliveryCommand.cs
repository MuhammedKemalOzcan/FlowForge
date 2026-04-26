using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.ProcessWebhookDelivery
{
    public record ProcessWebhookDeliveryCommand(Guid DeliveryId, Guid TenantId) : IRequest<Result>;
}