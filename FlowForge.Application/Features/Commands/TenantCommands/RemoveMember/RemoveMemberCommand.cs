using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.RemoveMember
{
    public record RemoveMemberCommand(Guid UserId, Guid TenantId) : IRequest<Result>;
}