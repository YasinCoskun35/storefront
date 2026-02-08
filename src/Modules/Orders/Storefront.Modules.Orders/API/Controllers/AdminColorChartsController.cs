using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Orders.Core.Application.Commands;

namespace Storefront.Modules.Orders.API.Controllers;

[ApiController]
[Route("api/admin/color-charts")]
[Authorize(Roles = "Admin")]
public class AdminColorChartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminColorChartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create new color chart
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateColorChart([FromBody] CreateColorChartRequest request)
    {
        var userId = System.Security.Claims.ClaimsPrincipal.Current?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var command = new CreateColorChartCommand(
            request.Name,
            request.Code,
            request.Description,
            request.Type,
            request.MainImageUrl,
            request.ThumbnailUrl,
            userId
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/admin/color-charts/{result.Value}", new { id = result.Value })
            : result.Error.Code == "ColorChart.CodeExists"
                ? Conflict(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Add color option to chart
    /// </summary>
    [HttpPost("{id}/options")]
    public async Task<IActionResult> AddColorOption(string id, [FromBody] AddColorOptionRequest request)
    {
        var command = new AddColorOptionCommand(
            id,
            request.Name,
            request.Code,
            request.HexColor,
            request.ImageUrl,
            request.PriceAdjustment,
            request.DisplayOrder
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/admin/color-charts/{id}/options/{result.Value}", new { id = result.Value })
            : result.Error.Code == "ColorChart.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : result.Error.Code == "ColorOption.CodeExists"
                    ? Conflict(new { error = result.Error.Code, message = result.Error.Message })
                    : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record CreateColorChartRequest(
    string Name,
    string Code,
    string Description,
    string Type,
    string? MainImageUrl,
    string? ThumbnailUrl
);

public record AddColorOptionRequest(
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    int DisplayOrder
);
