using System.Text;
using System.Text.RegularExpressions;

namespace Storefront.Modules.Catalog.Core.Application.Utilities;

/// <summary>
/// Utility class for generating URL-friendly slugs with Turkish character support
/// </summary>
public static class SlugGenerator
{
    private static readonly Dictionary<char, string> TurkishCharMap = new()
    {
        { 'ı', "i" },
        { 'İ', "i" },
        { 'ğ', "g" },
        { 'Ğ', "g" },
        { 'ü', "u" },
        { 'Ü', "u" },
        { 'ş', "s" },
        { 'Ş', "s" },
        { 'ö', "o" },
        { 'Ö', "o" },
        { 'ç', "c" },
        { 'Ç', "c" }
    };

    /// <summary>
    /// Generates a URL-friendly slug from the given text
    /// Properly handles Turkish characters (ı, ğ, ü, ş, ö, ç, İ)
    /// </summary>
    /// <param name="text">The text to convert to a slug</param>
    /// <returns>A URL-friendly slug</returns>
    /// <example>
    /// SlugGenerator.Generate("Özel Ürün") // "ozel-urun"
    /// SlugGenerator.Generate("Çok Güzel Şeyler") // "cok-guzel-seyler"
    /// SlugGenerator.Generate("İstanbul'da Kahve") // "istanbulda-kahve"
    /// </example>
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase first
        var slug = text.ToLowerInvariant();

        // Replace Turkish characters
        var sb = new StringBuilder();
        foreach (var ch in slug)
        {
            if (TurkishCharMap.TryGetValue(ch, out var replacement))
            {
                sb.Append(replacement);
            }
            else
            {
                sb.Append(ch);
            }
        }
        slug = sb.ToString();

        // Remove diacritics (accents) from other characters
        slug = RemoveDiacritics(slug);

        // Replace common special characters
        slug = slug
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "");

        // Remove any remaining non-alphanumeric characters (except hyphens and spaces)
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Convert multiple spaces/hyphens into single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
