namespace Storefront.Modules.Content.Core.Domain.ValueObjects;

public class SeoMetadata
{
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public string? OgImage { get; set; }
    public string? OgType { get; set; }
    public string? CanonicalUrl { get; set; }

    public static SeoMetadata Empty() => new();

    public static SeoMetadata FromContent(string title, string? description)
    {
        return new SeoMetadata
        {
            MetaTitle = title,
            MetaDescription = description?.Length > 160 
                ? description.Substring(0, 160) + "..." 
                : description
        };
    }
}

