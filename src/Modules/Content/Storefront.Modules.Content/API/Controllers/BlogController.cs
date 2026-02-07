using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Content.Core.Application.Commands;
using Storefront.Modules.Content.Core.Application.Queries;

namespace Storefront.Modules.Content.API.Controllers;

[ApiController]
[Route("api/content/blog")]
public sealed class BlogController : ControllerBase
{
    private readonly IMediator _mediator;

    public BlogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Public endpoint: Get paginated list of blog posts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBlogPosts(
        [FromQuery] string? category,
        [FromQuery] string? tag,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBlogPostsQuery(category, tag, IsPublished: true, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Admin endpoint: Create new blog post
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBlogPost(
        [FromBody] CreateBlogPostCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
        }

        return CreatedAtAction(nameof(GetBlogPosts), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Admin endpoint: Update existing blog post
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBlogPost(
        string id,
        [FromBody] UpdateBlogPostCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { error = "IdMismatch", message = "URL ID does not match body ID." });
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
        }

        return Ok(new { id = result.Value });
    }
}

