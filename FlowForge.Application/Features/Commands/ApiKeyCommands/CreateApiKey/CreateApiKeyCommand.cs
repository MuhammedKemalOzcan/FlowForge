using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.ApiKeyCommands.CreateApiKey
{
    public record CreateApiKeyCommand(Guid TenantId,string Name) : IRequest<Result<ApiKeyCreationResultDto>>;
}