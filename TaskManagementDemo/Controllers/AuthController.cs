﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                    return Unauthorized(new { error = "Authentication failed" });

                var userId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                    return Unauthorized(new { error = "Required claims missing" });

                var token = _authService.GenerateJwtToken(userId, email);

                // Return in a format that works well with the redirect
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // Log the error details
                return StatusCode(500, new { error = "Authentication failed", details = ex.Message });
            }
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return Unauthorized();

            var userId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return Unauthorized();

            var token = _authService.GenerateJwtToken(userId, email);
            return Ok(new { token });
        }
    }
}