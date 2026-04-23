using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointName
{
    public record ChangeEndpointNameCommand(string EndpointName, Guid EndpointId, Guid TenantId) : IRequest<Result>;
}