using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.Interfaces;
using Storefront.Modules.Content.Infrastructure.Persistence;

namespace Storefront.Modules.Content.Infrastructure.Services;

public sealed class SlugService : ISlugService
{
    private readonly ContentDbContext _context;

    public SlugService(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateUniqueSlugAsync(string text, string? entityId = null, CancellationToken cancellationToken = default)
    {
        var baseSlug = GenerateSlug(text);
        var slug = baseSlug;
        var counter = 1;

        // Check uniqueness across both BlogPosts and StaticPages
        while (await IsSlugTakenAsync(slug, entityId, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Remove accents and diacritics
        slug = RemoveDiacritics(slug);

        // Replace special characters with dashes
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace multiple spaces/dashes with single dash
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        // Trim dashes from start and end
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 200)
        {
            slug = slug.Substring(0, 200).TrimEnd('-');
        }

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private async Task<bool> IsSlugTakenAsync(string slug, string? entityId, CancellationToken cancellationToken)
    {
        // Check if slug exists in BlogPosts (excluding current entity if updating)
        var blogPostExists = await _context.BlogPosts
            .AnyAsync(bp => bp.Slug == slug && (entityId == null || bp.Id != entityId), cancellationToken);

        if (blogPostExists)
            return true;

        // Check if slug exists in StaticPages (excluding current entity if updating)
        var staticPageExists = await _context.StaticPages
            .AnyAsync(sp => sp.Slug == slug && (entityId == null || sp.Id != entityId), cancellationToken);

        return staticPageExists;
    }
}

