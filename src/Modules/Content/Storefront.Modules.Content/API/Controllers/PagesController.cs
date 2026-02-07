using MediatR;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Content.Core.Application.Queries;

namespace Storefront.Modules.Content.API.Controllers;

[ApiController]
[Route("api/content/pages")]
public sealed class PagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Public endpoint: Get static page by slug
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPageBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var query = new GetPageBySlugQuery(slug);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Public endpoint: Get sitemap entries for all content
    /// </summary>
    [HttpGet("sitemap")]
    public async Task<IActionResult> GetSitemap(CancellationToken cancellationToken)
    {
        var query = new GetContentSitemapQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }
}

