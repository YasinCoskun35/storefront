using MediatR;
using Microsoft.AspNetCore.Identity;
using Storefront.Modules.Identity.Core.Application.DTOs;
using Storefront.Modules.Identity.Core.Application.Interfaces;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<LoginResponse>.Failure(Error.NotFound("User.NotFound", "Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure(Error.Validation("User.Inactive", "User account is inactive."));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Result<LoginResponse>.Failure(Error.Validation("User.InvalidCredentials", "Invalid email or password."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);

        var response = new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
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

