using MediatR;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Catalog.Core.Application.Commands;
using Storefront.Modules.Catalog.Core.Application.Queries;

namespace Storefront.Modules.Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(
        [FromQuery] string? parentId,
        [FromQuery] bool? isActive = true,
        [FromQuery] bool? showInNavbar = null,
        [FromQuery] bool all = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(parentId, isActive, showInNavbar, all);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
        }

        return CreatedAtAction(nameof(GetCategories), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id, request.Name, request.Description, request.Slug,
            request.ParentId, request.DisplayOrder ?? 0, request.IsActive ?? true, request.ShowInNavbar ?? false);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
        }

        return NoContent();
    }
}

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? Slug,
    string? ParentId,
    int? DisplayOrder,
    bool? IsActive,
    bool? ShowInNavbar
);

