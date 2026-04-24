using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.AddMember
{
    public record AddMemberToTenantCommand(Guid UserId, Roles Role, Guid TenantId) : IRequest<Result>;
}