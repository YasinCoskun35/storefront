namespace Storefront.Modules.Content.Core.Application.DTOs;

public sealed record AppSettingDto(
    string Key,
    string Value,
    string DisplayName,
    string? Description,
    string Category,
    string DataType,
    DateTime UpdatedAt,
    string? UpdatedBy
);

public sealed record AppSettingGroupDto(
    string Category,
    List<AppSettingDto> Settings
);
