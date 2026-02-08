using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class AddColorOptionCommandHandler : IRequestHandler<AddColorOptionCommand, Result<string>>
{
    private readonly OrdersDbContext _context;

    public AddColorOptionCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AddColorOptionCommand request, CancellationToken cancellationToken)
    {
        // Verify chart exists
        var chartExists = await _context.ColorCharts.AnyAsync(cc => cc.Id == request.ColorChartId, cancellationToken);
        
        if (!chartExists)
        {
            return Error.NotFound("ColorChart.NotFound", "Color chart not found");
        }

        // Check for duplicate code within chart
        var codeExists = await _context.ColorOptions
            .AnyAsync(co => co.ColorChartId == request.ColorChartId && co.Code == request.Code, cancellationToken);

        if (codeExists)
        {
            return Error.Conflict("ColorOption.CodeExists", $"Color code '{request.Code}' already exists in this chart");
        }

        var colorOption = new ColorOption
        {
            ColorChartId = request.ColorChartId,
            Name = request.Name,
            Code = request.Code,
            HexColor = request.HexColor,
            ImageUrl = request.ImageUrl,
            PriceAdjustment = request.PriceAdjustment,
            DisplayOrder = request.DisplayOrder,
            IsAvailable = true,
            StockLevel = 0, // Unlimited by default
            CreatedAt = DateTime.UtcNow
        };

        _context.ColorOptions.Add(colorOption);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(colorOption.Id);
    }
}
