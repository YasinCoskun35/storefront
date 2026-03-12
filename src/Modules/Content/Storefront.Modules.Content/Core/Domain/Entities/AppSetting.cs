namespace Storefront.Modules.Content.Core.Domain.Entities;

/// <summary>
/// Application settings/feature flags
/// </summary>
public sealed class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string DataType { get; set; } = "boolean"; // boolean, string, number, json
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
