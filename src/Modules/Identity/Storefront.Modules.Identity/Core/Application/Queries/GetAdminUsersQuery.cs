using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public record AdminUserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    List<string> Roles,
    DateTime CreatedAt
);

public record GetAdminUsersQuery(string? SearchTerm = null) : IRequest<Result<List<AdminUserDto>>>;
