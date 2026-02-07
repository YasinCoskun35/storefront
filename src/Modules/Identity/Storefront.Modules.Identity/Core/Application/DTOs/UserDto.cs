namespace Storefront.Modules.Identity.Core.Application.DTOs;

public sealed record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IReadOnlyList<string> Roles
);

