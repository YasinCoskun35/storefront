using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Infrastructure.Persistence;

namespace Storefront.Modules.Content.API.Controllers;

[ApiController]
[Route("api/content/home-sliders")]
public sealed class HomeSlidersController : ControllerBase
{
    private readonly ContentDbContext _context;

    public HomeSlidersController(ContentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Public endpoint: Get all home slider data (hero slides, category slides, featured brands)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var heroSlides = await _context.HeroSlides
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Subtitle,
                s.ImageUrl,
                s.Link,
                s.LinkText,
            })
            .ToListAsync(cancellationToken);

        var categorySlides = await _context.HomeCategorySlides
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Slug,
                s.ImageUrl,
                s.Link,
                s.ProductCount,
            })
            .ToListAsync(cancellationToken);

        var featuredBrands = await _context.FeaturedBrands
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.LogoUrl,
                b.Link,
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            heroSlides,
            categorySlides,
            featuredBrands,
        });
    }
}
