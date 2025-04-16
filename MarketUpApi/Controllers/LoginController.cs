using MarketUpApi.Models;
using MarketUpApi.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketUpApi.Controllers
{
    public class LoginController : BaseApiController
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [HttpPost]      
        [ProducesDefaultResponseType(typeof(ApiResponse<LoginModel>))]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                _logger.LogInformation("Started");
                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, "Not Found");
            }

            return Ok();

        }
    }
}
