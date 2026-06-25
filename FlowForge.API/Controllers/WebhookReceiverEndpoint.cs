using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookReceiverEndpoint : ControllerBase
    {
        [HttpPost("success")]
        public IActionResult Success([FromBody] object payload)
        {
            return Ok(new
            {
                received = true,
                message = "Demo webhook received successfully."
            });
        }

        [HttpPost("fail")]
        public IActionResult Fail([FromBody] object payload)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                received = true,
                message = "This demo endpoint intentionally returns HTTP 500."
            });
        }
    }
}
