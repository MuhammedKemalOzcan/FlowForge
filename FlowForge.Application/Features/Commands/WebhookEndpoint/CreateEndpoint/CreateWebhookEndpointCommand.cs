using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.CreateEndpoint
{

    //TenantId'yi tokendan al.
    public record CreateWebhookEndpointCommand(string EndpointName, string TargetUrl, List<string> EventTypes, int MaxAttempts, BackoffStrategy Strategy, TimeSpan InitialDelay, TimeSpan MaxDelay, TimeSpan Timeout) : IRequest<Result<Guid>>
    {
    }
}