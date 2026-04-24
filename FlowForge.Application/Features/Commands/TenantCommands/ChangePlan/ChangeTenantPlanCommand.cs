using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.TenantCommands.ChangePlan
{
    public record ChangeTenantPlanCommand(Guid TenantId, Plan newPlan) : IRequest<Result>;
}