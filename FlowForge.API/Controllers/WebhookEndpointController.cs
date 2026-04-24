using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointName;
using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointRetryPolicy;
using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointSubscriptions;
using FlowForge.Application.Features.Commands.WebhookEndpoint.ChangeEndpointUrl;
using FlowForge.Application.Features.Commands.WebhookEndpoint.CreateEndpoint;
using FlowForge.Application.Features.Commands.WebhookEndpoint.RemoveEndpoint;
using FlowForge.Application.Features.Queries.WebhookEndpointQuery.GetAllEndpoints;
using FlowForge.Domain.Enums;
using MediatR;
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
        public async Task<IActionResult> GetEndpoints([FromQuery] GetAllEndpointsQuery query)
        {
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEndpoint([FromBody] CreateWebhookEndpointCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        //Jwt eklendiğinde kaldırılacak tenantId Jwt üzerinden alınacak.
        public record ChangeEndpointNameRequest(Guid TenantId, string EndpointName);

        [HttpPatch("{endpointId}/name")]
        public async Task<IActionResult> ChangeEndpointName([FromBody] ChangeEndpointNameRequest request, [FromRoute] Guid endpointId)
        {
            var command = new ChangeEndpointNameCommand(request.EndpointName, endpointId, request.TenantId);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        public record ChangeRetryPolicyRequest(
            Guid TenantId,
            int MaxAttempts,
            BackoffStrategy Strategy,
            TimeSpan InitialDelay,
            TimeSpan MaxDelay,
            TimeSpan TimeOut
            );

        [HttpPatch("{endpointId}/retryPolicy")]
        public async Task<IActionResult> ChangeRetryPolicy([FromRoute] Guid endpointId, [FromBody] ChangeRetryPolicyRequest request)
        {
            var command = new ChangeEndpointRetryPolicyCommand(
                endpointId,
                request.TenantId,
                request.MaxAttempts,
                request.Strategy,
                request.InitialDelay,
                request.MaxDelay,
                request.TimeOut);

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        public record ChangeEndpointSubscriptionRequest(Guid TenantId, List<string> EventTypes);

        [HttpPatch("{endpointId}/subscriptions")]
        public async Task<IActionResult> ChangeEndpointSubscriptions([FromRoute] Guid endpointId, [FromBody] ChangeEndpointSubscriptionRequest request)
        {
            var command = new ChangeEndpointSubscriptionsCommand(endpointId, request.TenantId, request.EventTypes);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        public record ChangeEndpointUrlRequest(Guid TenantId, string Url);

        [HttpPatch("{endpointId}/url")]
        public async Task<IActionResult> ChangeEndpointUrl([FromBody] ChangeEndpointUrlRequest request, [FromRoute] Guid endpointId)
        {
            var command = new ChangeEndpointUrlCommand(request.Url, endpointId, request.TenantId);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpDelete("{endpointId}")]
        public async Task<IActionResult> RemoveEndpoint([FromBody] Guid tenantId, [FromRoute] Guid endpointId)
        {
            var command = new RemoveEndpointCommand(tenantId, endpointId);
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