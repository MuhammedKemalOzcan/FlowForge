using FlowForge.Application.Features.Commands.TenantCommands.CreateTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : BaseApiController
    {
        private readonly IMediator _mediator;

        public TenantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }


    }
}