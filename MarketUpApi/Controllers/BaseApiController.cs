using MarketUpApi.Rest;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketUpApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Produces(ApiResponseType.JsonResponse)]
    public abstract class BaseApiController : ControllerBase
    {
        protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        protected new IActionResult Ok(object value = null)
        {
            var response = new ApiResponse
            {
                Data = value,
                Success = true
            };

            return base.Ok(response);
        }

    }
}
