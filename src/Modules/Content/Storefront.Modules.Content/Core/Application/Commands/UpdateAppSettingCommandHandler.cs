using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Commands;

public sealed class UpdateAppSettingCommandHandler : IRequestHandler<UpdateAppSettingCommand, Result>
{
    private readonly ContentDbContext _context;

    public UpdateAppSettingCommandHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAppSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (setting == null)
        {
            return Result.Failure(Error.NotFound("Setting.NotFound", $"Setting with key '{request.Key}' not found."));
        }

        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
