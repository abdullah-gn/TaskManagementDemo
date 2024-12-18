using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using TaskManagementDemo.Services;

namespace TaskManagementDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                Items =
            {
                { ".redirect", "/api/Auth/google-callback" },
                { "LoginProvider", "Google" }
            },
                AllowRefresh = true,
                IsPersistent = true
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Authentication failed" });
                }

                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
                var userId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message = "User information not found" });
                }

                var token = _authService.GenerateJwtToken(userId, email);

                // Return JSON response that can be handled by the client
                return Ok(new
                {
                    token = token,
                    email = email,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during authentication", error = ex.Message });
            }
        }
    }
}
