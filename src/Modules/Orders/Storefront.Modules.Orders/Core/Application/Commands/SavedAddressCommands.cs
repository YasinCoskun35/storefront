using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

// ── Create ──────────────────────────────────────────────────────────────────

public sealed record CreateSavedAddressCommand(
    string PartnerUserId,
    string PartnerCompanyId,
    string Label,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault
) : IRequest<Result<string>>;

public class CreateSavedAddressCommandHandler : IRequestHandler<CreateSavedAddressCommand, Result<string>>
{
    private readonly OrdersDbContext _context;

    public CreateSavedAddressCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateSavedAddressCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            // Clear existing default for this user
            await _context.SavedAddresses
                .Where(a => a.PartnerUserId == request.PartnerUserId && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), cancellationToken);
        }

        var address = new SavedAddress
        {
            PartnerUserId = request.PartnerUserId,
            PartnerCompanyId = request.PartnerCompanyId,
            Label = request.Label,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        _context.SavedAddresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(address.Id);
    }
}

// ── Delete ───────────────────────────────────────────────────────────────────

public sealed record DeleteSavedAddressCommand(
    string AddressId,
    string PartnerUserId
) : IRequest<Result>;

public class DeleteSavedAddressCommandHandler : IRequestHandler<DeleteSavedAddressCommand, Result>
{
    private readonly OrdersDbContext _context;

    public DeleteSavedAddressCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteSavedAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _context.SavedAddresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.PartnerUserId == request.PartnerUserId, cancellationToken);

        if (address is null)
            return Error.NotFound("SavedAddress.NotFound", "Address not found.");

        _context.SavedAddresses.Remove(address);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
