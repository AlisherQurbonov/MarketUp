using Microsoft.AspNetCore.Mvc;

namespace MarketUpApi.Controllers
{
    public class MetricsController : BaseApiController
    {
        public MetricsController()
        {
            
        }

        [HttpGet]
        public IActionResult Metrics()
        {
            return Ok();
        }
    }
}
