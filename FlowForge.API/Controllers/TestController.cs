using FlowForge.Application.Abstractions;
using FlowForge.Infrastructure.Authentication;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : BaseApiController
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly IMediator _mediator;

        public TestController(ICurrentTenant currentTenant, IMediator mediator)
        {
            _currentTenant = currentTenant;
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult Get()
        {
            return Ok(new
            {
                tenantId = _currentTenant.TenantId,
                apiKeyId = _currentTenant.ApiKeyId
            });
        }

        //public record ProcessDeliveryRequest(Guid TenantId);

        //[HttpPost("{deliveryId}/process")]
        //public async Task<IActionResult> ProcessDelivery(
        //[FromRoute] Guid deliveryId,
        //[FromBody] ProcessDeliveryRequest request)
        //{
        //    var command = new ProcessWebhookDeliveryCommand(deliveryId, request.TenantId);
        //    var result = await _mediator.Send(command);
        //    return HandleResult(result);
        //}
    }
}