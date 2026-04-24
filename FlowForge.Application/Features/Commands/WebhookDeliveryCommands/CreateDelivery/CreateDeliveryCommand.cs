using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookDeliveryCommands.CreateDelivery
{
    public record CreateDeliveryCommand(Guid TenantId, Guid EndpointId, string EventType, string Payload, string IdempotencyKey) : IRequest<Result<Guid>>;
}