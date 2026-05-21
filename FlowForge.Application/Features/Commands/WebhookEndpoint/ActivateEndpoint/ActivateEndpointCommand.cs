using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.WebhookEndpoint.ActivateEndpoint;

public record ActivateEndpointCommand(Guid Id) : IRequest<Result>;
