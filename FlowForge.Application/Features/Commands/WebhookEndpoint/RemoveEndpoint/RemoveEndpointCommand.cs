using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.RemoveEndpoint
{
    public record RemoveEndpointCommand(Guid TenantId, Guid Id) : IRequest<Result>;
}