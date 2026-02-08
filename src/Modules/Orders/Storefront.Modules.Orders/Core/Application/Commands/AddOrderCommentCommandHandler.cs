using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class AddOrderCommentCommandHandler : IRequestHandler<AddOrderCommentCommand, Result<string>>
{
    private readonly OrdersDbContext _context;

    public AddOrderCommentCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AddOrderCommentCommand request, CancellationToken cancellationToken)
    {
        // Verify order exists
        var orderExists = await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken);
        
        if (!orderExists)
        {
            return Error.NotFound("Order.NotFound", "Order not found");
        }

        var comment = new OrderComment
        {
            OrderId = request.OrderId,
            Content = request.Content,
            Type = request.Type,
            AuthorId = request.AuthorId,
            AuthorName = request.AuthorName,
            AuthorType = request.AuthorType,
            IsInternal = request.IsInternal,
            IsSystemGenerated = false,
            AttachmentUrl = request.AttachmentUrl,
            AttachmentFileName = request.AttachmentFileName,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(comment.Id);
    }
}
