using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Commands;

public sealed record UpdateAppSettingCommand(
    string Key,
    string Value,
    string? UpdatedBy = null
) : IRequest<Result>;
