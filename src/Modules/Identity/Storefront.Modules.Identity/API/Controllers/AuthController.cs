using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Storefront.Modules.Identity.Core.Application.Commands;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "NotFound"   => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _            => StatusCode(500, new { error = "InternalError", message = "An unexpected error occurred." })
            };
        }

        var cookieName = _configuration["Cookie:AdminCookieName"] ?? "admin_token";
        Response.Cookies.Append(cookieName, result.Value.AccessToken, BuildCookieOptions(result.Value.ExpiresAt));

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "NotFound"   => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _            => StatusCode(500, new { error = "InternalError", message = "An unexpected error occurred." })
            };
        }

        var cookieName = _configuration["Cookie:AdminCookieName"] ?? "admin_token";
        Response.Cookies.Append(cookieName, result.Value.AccessToken, BuildCookieOptions(result.Value.ExpiresAt));

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var cookieName = _configuration["Cookie:AdminCookieName"] ?? "admin_token";
        Response.Cookies.Delete(cookieName);
        return NoContent();
    }

    private CookieOptions BuildCookieOptions(DateTime expiresAt)
    {
        var sameSiteStr    = _configuration["Cookie:SameSite"]    ?? "Strict";
        var securePolicyStr = _configuration["Cookie:SecurePolicy"] ?? "Always";

        var sameSite = sameSiteStr.Equals("Strict", StringComparison.OrdinalIgnoreCase)
            ? SameSiteMode.Strict
            : SameSiteMode.Lax;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure   = securePolicyStr.Equals("Always", StringComparison.OrdinalIgnoreCase),
            SameSite = sameSite,
            Expires  = expiresAt,
            Path     = "/"
        };
    }
}
