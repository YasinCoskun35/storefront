using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, Result<List<AdminUserDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAdminUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<List<AdminUserDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var users = await query.OrderBy(u => u.Email).ToListAsync(cancellationToken);

        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new AdminUserDto(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.IsActive,
                roles.ToList(),
                user.CreatedAt
            ));
        }

        return Result<List<AdminUserDto>>.Success(result);
    }
}
