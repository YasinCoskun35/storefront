using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class CreateColorChartCommandHandler : IRequestHandler<CreateColorChartCommand, Result<string>>
{
    private readonly OrdersDbContext _context;

    public CreateColorChartCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateColorChartCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var existingChart = await _context.ColorCharts
            .FirstOrDefaultAsync(cc => cc.Code == request.Code, cancellationToken);

        if (existingChart is not null)
        {
            return Error.Conflict("ColorChart.CodeExists", $"Color chart with code '{request.Code}' already exists");
        }

        var colorChart = new ColorChart
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Type = request.Type,
            MainImageUrl = request.MainImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.ColorCharts.Add(colorChart);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(colorChart.Id);
    }
}
