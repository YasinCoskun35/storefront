using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Application.Queries;
using System.Security.Claims;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/partners")]
public class PartnerAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public PartnerAuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// Partner login
    /// </summary>
    [HttpPost("auth/login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] PartnerLoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Message });

        var cookieName = _configuration["Cookie:PartnerCookieName"] ?? "partner_token";
        var expiresAt  = DateTime.UtcNow.AddSeconds(result.Value.ExpiresIn);
        Response.Cookies.Append(cookieName, result.Value.AccessToken, BuildCookieOptions(expiresAt));

        return Ok(result.Value);
    }

    /// <summary>
    /// Refresh partner access token — issues a new JWT while the current one is still valid.
    /// </summary>
    [HttpPost("auth/refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var result = await _mediator.Send(new RefreshPartnerTokenCommand(userId), cancellationToken);

        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });

        var cookieName = _configuration["Cookie:PartnerCookieName"] ?? "partner_token";
        var expiresAt  = DateTime.UtcNow.AddSeconds(result.Value.ExpiresIn);
        Response.Cookies.Append(cookieName, result.Value.AccessToken, BuildCookieOptions(expiresAt));

        return Ok(result.Value);
    }

    /// <summary>
    /// Request a password reset email. Always returns 200 to avoid leaking account existence.
    /// </summary>
    [HttpPost("auth/forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest body,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Email))
            return BadRequest(new { error = "Validation", message = "Email is required." });

        await _mediator.Send(new RequestPartnerPasswordResetCommand(body.Email), cancellationToken);
        return Ok(new { message = "If the account exists, a reset email has been sent." });
    }

    /// <summary>
    /// Reset password using the token from the reset email
    /// </summary>
    [HttpPost("auth/reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] PartnerResetPasswordRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ResetPartnerPasswordCommand(body.Token, body.NewPassword), cancellationToken);

        return result.IsSuccess
            ? Ok(new { message = "Password has been reset. You can now sign in." })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Partner logout — clears the httpOnly auth cookie
    /// </summary>
    [HttpPost("auth/logout")]
    public IActionResult Logout()
    {
        var cookieName = _configuration["Cookie:PartnerCookieName"] ?? "partner_token";
        Response.Cookies.Delete(cookieName);
        return NoContent();
    }

    /// <summary>
    /// Register or update the Expo push token for the current partner user
    /// </summary>
    [HttpPost("push-token")]
    [Authorize]
    public async Task<IActionResult> RegisterPushToken(
        [FromBody] RegisterPushTokenRequest body,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var result = await _mediator.Send(new RegisterPushTokenCommand(userId, body.PushToken), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : NotFound(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Get current partner profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Partner")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var result = await _mediator.Send(new GetPartnerProfileQuery(userId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Get current partner's account balance and transaction history
    /// </summary>
    [HttpGet("account")]
    [Authorize(Roles = "Partner")]
    public async Task<IActionResult> GetAccount(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var result = await _mediator.Send(new GetPartnerAccountQuery(userId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Code, message = result.Error.Message });
    }

    private CookieOptions BuildCookieOptions(DateTime expiresAt)
    {
        var sameSiteStr     = _configuration["Cookie:SameSite"]     ?? "Strict";
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

public record RegisterPushTokenRequest(string? PushToken);
public record ForgotPasswordRequest(string Email);
public record PartnerResetPasswordRequest(string Token, string NewPassword);
