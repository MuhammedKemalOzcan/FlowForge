using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointSubscriptions
{
    public record ChangeEndpointSubscriptionsCommand(Guid EndpointId, Guid TenantId, List<string> EventTypes) : IRequest<Result>;
}