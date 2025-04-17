using MarketUpApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MarketUpApi.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel userLogin)
        {
            var token = "";

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Model not found");
                }

                if (userLogin.UserName == "testuser" && userLogin.Password == "password")
                {
                    _logger.LogInformation("Generate JWT");
                    token = GenerateJwtToken(userLogin.UserName);
                }
                else 
                {
                    _logger.LogError("UserName or password not found");
                    return Unauthorized();
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }
       
            return Ok(
                new 
                {
                    token 
                });
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
