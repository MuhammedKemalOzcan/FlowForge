using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.CreateTenant
{
    public record CreateTenantCommand(Guid UserId, string Name) : IRequest<Result<Guid>>;
}