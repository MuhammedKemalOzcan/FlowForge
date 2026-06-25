using FlowForge.Application.Features.Commands.DemoCommands.BootstrapDemo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    public class DemoController : BaseApiController
    {
        private readonly IMediator _mediator;

        public DemoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Bootstrap(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BootstrapDemoCommand(), cancellationToken);
            return HandleResult(result);
        }
    }
}
