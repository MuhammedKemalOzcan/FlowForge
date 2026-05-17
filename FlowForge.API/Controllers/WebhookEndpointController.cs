using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointName;
using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointRetryPolicy;
using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointSubscriptions;
using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointUrl;
using FlowForge.Application.Features.Commands.WebhookEndpoint.CreateEndpoint;
using FlowForge.Application.Features.Commands.WebhookEndpoint.RemoveEndpoint;
using FlowForge.Application.Features.Queries.WebhookEndpointQuery.GetAllEndpoints;
using FlowForge.Domain.Enums;
using FlowForge.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookEndpointController : BaseApiController
    {
        private readonly IMediator _mediator;

        public WebhookEndpointController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetEndpoints([FromQuery] GetAllEndpointsQuery query)
        {
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateEndpoint([FromBody] CreateWebhookEndpointCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        //Jwt eklendiğinde kaldırılacak tenantId Jwt üzerinden alınacak.

        [HttpPatch("{endpointId}/name")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeEndpointName([FromBody] string endpointName, [FromRoute] Guid endpointId)
        {
            var command = new ChangeEndpointNameCommand(endpointName, endpointId);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        public record ChangeRetryPolicyRequest(
            int MaxAttempts,
            BackoffStrategy Strategy,
            TimeSpan InitialDelay,
            TimeSpan MaxDelay,
            TimeSpan TimeOut
            );

        [HttpPatch("{endpointId}/retry-policy")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeRetryPolicy([FromRoute] Guid endpointId, [FromBody] ChangeRetryPolicyRequest request)
        {
            var command = new ChangeEndpointRetryPolicyCommand(
                endpointId,
                request.MaxAttempts,
                request.Strategy,
                request.InitialDelay,
                request.MaxDelay,
                request.TimeOut);

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPatch("{endpointId}/subscriptions")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeEndpointSubscriptions([FromRoute] Guid endpointId, [FromBody] List<string> eventTypes)
        {
            var command = new ChangeEndpointSubscriptionsCommand(endpointId, eventTypes);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPatch("{endpointId}/url")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeEndpointUrl([FromBody] string url, [FromRoute] Guid endpointId)
        {
            var command = new ChangeEndpointUrlCommand(url, endpointId);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpDelete("{endpointId}")]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RemoveEndpoint([FromRoute] RemoveEndpointCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        //public record SubscribeEventTypeRequest(Guid TenantId, string EventType);

        //[HttpPost("{endpointId}/subscriptions")]
        //public async Task<IActionResult> SubscribeEventType(
        //    [FromRoute] Guid endpointId,
        //    [FromBody] SubscribeEventTypeRequest request)
        //{
        //    // Handler'ın yok — şimdilik bırak, ileride yaz
        //}
    }
}