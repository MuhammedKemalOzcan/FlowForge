using FlowForge.Application.Features.Commands.ApiKeyCommands.CreateApiKey;
using FlowForge.Application.Features.Commands.ApiKeyCommands.RevokeApiKey;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiKeyController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ApiKeyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateApiKeyCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(RevokeApiKeyCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}