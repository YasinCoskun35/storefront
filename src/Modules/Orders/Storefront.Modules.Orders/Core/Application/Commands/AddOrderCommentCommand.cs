using MediatR;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record AddOrderCommentCommand(
    string OrderId,
    string Content,
    CommentType Type,
    string AuthorId,
    string AuthorName,
    string AuthorType, // "Admin" or "Partner"
    bool IsInternal,
    string? AttachmentUrl,
    string? AttachmentFileName
) : IRequest<Result<string>>;
