using FlowForge.Application.Features.Commands.WebhookDeliveryCommands.CreateDelivery;
using FlowForge.Application.Features.Commands.WebhookDeliveryCommands.RequeueDeadLetteredDelivery;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeadLetteredDeliveries;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeliveryById;
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

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> GetDelivery([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetDeliveryByIdQuery(id));
            return HandleResult(result);
        }

        [HttpGet("deadletter")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> GetDeadLetteredDeliveries()
        {
            var result = await _mediator.Send(new GetDeadLetteredDeliveriesQuery());
            return HandleResult(result);
        }

        [HttpPost("{id}/requeue")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> RequeueDelivery([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new RequeueDeadLetteredDeliveryCommand(id));
            return HandleResult(result);
        }

    }
}