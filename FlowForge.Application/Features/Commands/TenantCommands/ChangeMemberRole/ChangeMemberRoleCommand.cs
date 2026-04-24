using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.ChangeMemberRole
{
    public record ChangeMemberRoleCommand(Guid UserId, Guid TenantId, Roles NewRole) : IRequest<Result>;
}