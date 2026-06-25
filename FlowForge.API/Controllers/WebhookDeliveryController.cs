using System.Text.Json;
using FlowForge.Application.Abstractions;
using FlowForge.Application.Features.Commands.WebhookDeliveryCommands.CreateDelivery;
using FlowForge.Application.Features.Commands.WebhookDeliveryCommands.RequeueDeadLetteredDelivery;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetAllDeliveries;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeadLetteredDeliveries;
using FlowForge.Application.Features.Queries.WebhookDeliveriesQuery.GetDeliveryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookDeliveryController : BaseApiController
    {
        private static readonly JsonSerializerOptions StreamJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IMediator _mediator;
        private readonly ICurrentTenant _currentTenant;
        private readonly IWebhookDeliveryStreamBroadcaster _streamBroadcaster;

        public WebhookDeliveryController(
            IMediator mediator,
            ICurrentTenant currentTenant,
            IWebhookDeliveryStreamBroadcaster streamBroadcaster)
        {
            _mediator = mediator;
            _currentTenant = currentTenant;
            _streamBroadcaster = streamBroadcaster;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> CreateWebhook(CreateDeliveryCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task<IActionResult> GetAllDeliveries([FromQuery] GetAllDeliveriesQuery query)
        {
            var result = await _mediator.Send(query);
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

        /// <summary>
        /// Server-Sent Events stream of webhook-delivery lifecycle events for the current tenant.
        /// Optionally narrowed to a single endpoint and/or delivery via query string.
        /// Authenticated via the X-Api-Key header (use a fetch-based SSE client to send it).
        /// </summary>
        [HttpGet("stream")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public async Task Stream([FromQuery] Guid? endpointId, [FromQuery] Guid? deliveryId, CancellationToken ct)
        {
            var tenantId = _currentTenant.GetRequiredTenantId();

            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no"; // disable proxy buffering

            HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

            using var subscription = _streamBroadcaster.Subscribe(tenantId, endpointId, deliveryId);

            // Initial frames: confirm the stream is open and hint the reconnect interval.
            await Response.WriteAsync(": connected\n\n", ct);
            await Response.WriteAsync("retry: 5000\n\n", ct);
            await Response.Body.FlushAsync(ct);

            var heartbeatInterval = TimeSpan.FromSeconds(20);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    heartbeatCts.CancelAfter(heartbeatInterval);

                    bool hasData;
                    try
                    {
                        hasData = await subscription.Reader.WaitToReadAsync(heartbeatCts.Token);
                    }
                    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                    {
                        // Heartbeat tick — keep the connection alive and detect dead clients.
                        await Response.WriteAsync(": keep-alive\n\n", ct);
                        await Response.Body.FlushAsync(ct);
                        continue;
                    }

                    if (!hasData)
                        break; // channel completed

                    while (subscription.Reader.TryRead(out var evt))
                    {
                        var json = JsonSerializer.Serialize(evt, StreamJsonOptions);
                        await Response.WriteAsync($"event: {evt.EventName}\n", ct);
                        await Response.WriteAsync($"data: {json}\n\n", ct);
                    }

                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected — normal termination.
            }
        }

    }
}