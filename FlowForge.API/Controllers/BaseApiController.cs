using FlowForge.Domain.Enums;
using FlowForge.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                if (result.Data == null) return NoContent();

                return Ok(result.Data);
            }

            return result.Error.ErrorType switch
            {
                ErrorType.Validation => BadRequest(result.Error),
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Conflict => Conflict(result.Error),
                ErrorType.LimitExceeded => StatusCode(429, result.Error),
                ErrorType.Forbidden => StatusCode(403, result.Error),
                _ => StatusCode(500, result.Error)
            };
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result == null) return NoContent();

            if (result.IsSuccess) return Ok();

            return result.Error.ErrorType switch
            {
                ErrorType.Validation => BadRequest(result.Error),
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Conflict => Conflict(result.Error),
                ErrorType.LimitExceeded => StatusCode(429, result.Error),
                ErrorType.Forbidden => StatusCode(403, result.Error),
                _ => StatusCode(500, result.Error)
            };
        }
    }
}