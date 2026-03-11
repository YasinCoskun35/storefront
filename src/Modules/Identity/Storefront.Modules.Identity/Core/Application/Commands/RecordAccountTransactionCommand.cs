using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record RecordAccountTransactionCommand(
    string CompanyId,
    TransactionType Type,
    decimal Amount,
    PaymentMethod? PaymentMethod,
    string? OrderReference,
    string? Notes,
    string CreatedBy
) : IRequest<Result<string>>;

public class RecordAccountTransactionCommandHandler : IRequestHandler<RecordAccountTransactionCommand, Result<string>>
{
    private readonly IdentityDbContext _context;

    public RecordAccountTransactionCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(RecordAccountTransactionCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Error.Validation("Transaction.InvalidAmount", "Amount must be greater than zero.");
        }

        var company = await _context.PartnerCompanies
            .FirstOrDefaultAsync(pc => pc.Id == request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Error.NotFound("Partner.NotFound", "Partner company not found.");
        }

        var transaction = new PartnerAccountTransaction
        {
            PartnerCompanyId = request.CompanyId,
            Type = request.Type,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            OrderReference = request.OrderReference,
            Notes = request.Notes,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        // OrderDebit increases balance (partner owes more), PaymentCredit decreases it
        company.CurrentBalance = request.Type switch
        {
            TransactionType.OrderDebit => company.CurrentBalance + request.Amount,
            TransactionType.PaymentCredit => company.CurrentBalance - request.Amount,
            TransactionType.ManualAdjustment => company.CurrentBalance + request.Amount,
            _ => company.CurrentBalance
        };

        _context.PartnerAccountTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(transaction.Id);
    }
}
