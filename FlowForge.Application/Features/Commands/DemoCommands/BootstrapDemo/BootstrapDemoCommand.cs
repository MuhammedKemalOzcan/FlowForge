using FlowForge.Application.Dtos;
using FlowForge.Domain.Errors;
using MediatR;

namespace FlowForge.Application.Features.Commands.DemoCommands.BootstrapDemo
{
    public record BootstrapDemoCommand() : IRequest<Result<DemoBootstrapResultDto>>;
}
