using MediatR;
using Microsoft.AspNetCore.Identity;
using Storefront.Modules.Identity.Core.Application.DTOs;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result<UserDto>.Failure(Error.Conflict("User.AlreadyExists", "A user with this email already exists."));
        }

        // Validate roles exist
        foreach (var roleName in request.Roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return Result<UserDto>.Failure(Error.Validation("Role.NotFound", $"Role '{roleName}' does not exist."));
            }
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure(Error.Validation("User.CreationFailed", errors));
        }

        // Assign roles
        var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure(Error.Validation("User.RoleAssignmentFailed", errors));
        }

        var userDto = new UserDto(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            IsActive: user.IsActive,
            Roles: request.Roles
        );

        return Result<UserDto>.Success(userDto);
    }
}

