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

                // Check if the request is from Swagger
                var isSwaggerRequest = Request.Headers["Referer"].FirstOrDefault()?.Contains("/swagger/") ?? false;

                if (isSwaggerRequest)
                {
                    // Return JSON response for Swagger
                    return Ok(new
                    {
                        token = token,
                        email = email,
                        userId = userId
                    });
                }
                else
                {
                    // Return HTML page for browser requests
                    var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Authentication Success</title>
                    <script>
                        function copyToken() {{
                            navigator.clipboard.writeText('{token}');
                            alert('Token copied to clipboard!');
                        }}
                    </script>
                </head>
                <body>
                    <h2>Authentication Successful!</h2>
                    <p>Your token:</p>
                    <textarea rows='10' cols='50' readonly>{token}</textarea>
                    <br/>
                    <button onclick='copyToken()'>Copy Token</button>
                    <p>You can now use this token in Swagger UI:</p>
                    <ol>
                        <li>Go back to <a href='/swagger'>Swagger UI</a></li>
                        <li>Click the 'Authorize' button</li>
                        <li>Enter 'Bearer {token}' in the value field</li>
                        <li>Click 'Authorize'</li>
                    </ol>
                </body>
                </html>";
                    return Content(html, "text/html");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during authentication", error = ex.Message });
            }
        }
    }
}
