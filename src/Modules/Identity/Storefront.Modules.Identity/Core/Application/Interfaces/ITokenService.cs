using Storefront.Modules.Identity.Core.Domain.Entities;

namespace Storefront.Modules.Identity.Core.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    Task<string> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}

