using FlowForge.Application.Abstractions;
using FlowForge.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
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
    }
}