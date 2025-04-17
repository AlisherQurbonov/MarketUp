using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MarketUpApi.Models
{
    public class LoginModel
    {
        [Required]
        [JsonProperty("userName")]      
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
