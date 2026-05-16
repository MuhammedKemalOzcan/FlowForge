using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.ApiKeyCommands.RevokeApiKey
{
    public record RevokeApiKeyCommand(Guid ApiKeyId, Guid TenantId) : IRequest<Result>;
}