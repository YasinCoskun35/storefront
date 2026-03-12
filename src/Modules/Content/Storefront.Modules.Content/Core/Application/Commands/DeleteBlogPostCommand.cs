using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Commands;

public record DeleteBlogPostCommand(string Id) : IRequest<Result>;

public class DeleteBlogPostCommandHandler : IRequestHandler<DeleteBlogPostCommand, Result>
{
    private readonly ContentDbContext _context;

    public DeleteBlogPostCommandHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBlogPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post is null)
            return Result.Failure(Error.NotFound("BlogPost.NotFound", "Blog post not found."));

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
