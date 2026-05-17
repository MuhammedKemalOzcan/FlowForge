using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointUrl
{
    public record ChangeEndpointUrlCommand(string Url, Guid EndpointId) : IRequest<Result>;
}