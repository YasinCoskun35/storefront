using MediatR;
using Microsoft.AspNetCore.Identity;
using Storefront.Modules.Identity.Core.Application.DTOs;
using Storefront.Modules.Identity.Core.Application.Interfaces;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken is null || !refreshToken.IsActive)
        {
            return Result<LoginResponse>.Failure(Error.Validation("RefreshToken.Invalid", "Invalid or expired refresh token."));
        }

        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        if (user is null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure(Error.NotFound("User.NotFound", "User not found or inactive."));
        }

        // Revoke the old refresh token
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);

        var response = new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            User: new UserDto(
                Id: user.Id,
                Email: user.Email!,
                FirstName: user.FirstName,
                LastName: user.LastName,
                IsActive: user.IsActive,
                Roles: roles.ToList()
            )
        );

        return Result<LoginResponse>.Success(response);
    }
}

