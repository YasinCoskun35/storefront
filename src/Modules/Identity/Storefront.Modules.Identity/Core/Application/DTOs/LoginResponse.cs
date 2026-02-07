namespace Storefront.Modules.Identity.Core.Application.DTOs;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

