using FlowForge.Application.Abstractions;
using FlowForge.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : BaseApiController
    {
        private readonly ICurrentTenant _currentTenant;


        public TestController(ICurrentTenant currentTenant)
        {
            _currentTenant = currentTenant;
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

        [HttpPost]
        public Task<IActionResult> ex()
        {
            throw new Exception("test");
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