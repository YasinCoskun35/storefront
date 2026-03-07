using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Content.Core.Application.Commands;
using Storefront.Modules.Content.Core.Application.Queries;

namespace Storefront.Modules.Content.API.Controllers;

[ApiController]
[Route("api/content/settings")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all application settings (public, no auth required)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var query = new GetAppSettingsQuery(category);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Update a setting (admin only)
    /// </summary>
    [HttpPut("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSetting(
        string key,
        [FromBody] UpdateSettingRequest request,
        CancellationToken cancellationToken)
    {
        // Get admin user ID from claims
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var command = new UpdateAppSettingCommand(key, request.Value, adminUserId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(new { message = "Setting updated successfully" })
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record UpdateSettingRequest(string Value);
