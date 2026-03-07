using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public sealed record GetSavedAddressesQuery(string PartnerUserId) : IRequest<Result<List<SavedAddressDto>>>;

public sealed record SavedAddressDto(
    string Id,
    string Label,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault
);

public class GetSavedAddressesQueryHandler : IRequestHandler<GetSavedAddressesQuery, Result<List<SavedAddressDto>>>
{
    private readonly OrdersDbContext _context;

    public GetSavedAddressesQueryHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SavedAddressDto>>> Handle(GetSavedAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _context.SavedAddresses
            .Where(a => a.PartnerUserId == request.PartnerUserId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new SavedAddressDto(
                a.Id,
                a.Label,
                a.Address,
                a.City,
                a.State,
                a.PostalCode,
                a.Country,
                a.IsDefault
            ))
            .ToListAsync(cancellationToken);

        return Result<List<SavedAddressDto>>.Success(addresses);
    }
}
