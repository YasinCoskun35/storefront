using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record DeleteVariantOptionCommand(string Id) : IRequest<Result>;

public sealed class DeleteVariantOptionCommandHandler : IRequestHandler<DeleteVariantOptionCommand, Result>
{
    private readonly CatalogDbContext _context;

    public DeleteVariantOptionCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteVariantOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _context.VariantOptions
            .FirstOrDefaultAsync(vo => vo.Id == request.Id, cancellationToken);

        if (option is null)
            return Result.Failure(Error.NotFound("VariantOption.NotFound", "Variant option not found."));

        _context.VariantOptions.Remove(option);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
