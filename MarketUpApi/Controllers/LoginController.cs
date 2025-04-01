using MarketUpApi.Models;
using MarketUpApi.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketUpApi.Controllers
{
    public class LoginController : BaseApiController
    {
        public LoginController()
        {
        }

        [Authorize]
        [HttpPost]      
        [ProducesDefaultResponseType(typeof(ApiResponse<LoginModel>))]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            return Ok();
        }
    }
}
