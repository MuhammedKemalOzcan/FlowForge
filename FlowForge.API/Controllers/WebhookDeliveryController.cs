using FlowForge.Application.Features.Commands.WebhookDeliveryCommands.CreateDelivery;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookDeliveryController : BaseApiController
    {
        private readonly IMediator _mediator;

        public WebhookDeliveryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> CreateWebhook(CreateDeliveryCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> ListWebhooks([FromQuery] WebhookDeliveryQuery query)
        {
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

    }
}