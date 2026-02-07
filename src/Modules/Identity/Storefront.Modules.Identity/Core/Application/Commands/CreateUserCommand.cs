using MediatR;
using Storefront.Modules.Identity.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    List<string> Roles
) : IRequest<Result<UserDto>>;

