using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public record PartnerAccountDto(
    decimal CurrentBalance,
    decimal DiscountRate,
    IReadOnlyList<PartnerAccountTransactionDto> Transactions
);

public record GetPartnerAccountQuery(string PartnerUserId) : IRequest<Result<PartnerAccountDto>>;

public class GetPartnerAccountQueryHandler : IRequestHandler<GetPartnerAccountQuery, Result<PartnerAccountDto>>
{
    private readonly IdentityDbContext _context;

    public GetPartnerAccountQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PartnerAccountDto>> Handle(GetPartnerAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.PartnerUsers
            .Include(u => u.Company)
            .ThenInclude(c => c.AccountTransactions)
            .FirstOrDefaultAsync(u => u.Id == request.PartnerUserId, cancellationToken);

        if (user is null)
            return Result<PartnerAccountDto>.Failure(Error.NotFound("PartnerUser.NotFound", "User not found."));

        var transactions = user.Company.AccountTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new PartnerAccountTransactionDto(
                t.Id,
                t.Type.ToString(),
                t.Amount,
                t.PaymentMethod.HasValue ? t.PaymentMethod.Value.ToString() : null,
                t.OrderReference,
                t.Notes,
                t.CreatedBy,
                t.CreatedAt
            ))
            .ToList();

        return Result<PartnerAccountDto>.Success(new PartnerAccountDto(
            user.Company.CurrentBalance,
            user.Company.DiscountRate,
            transactions
        ));
    }
}
