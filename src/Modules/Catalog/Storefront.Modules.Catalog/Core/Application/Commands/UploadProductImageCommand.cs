using MediatR;
using Microsoft.AspNetCore.Http;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record UploadProductImageCommand(
    string ProductId,
    IFormFile File,
    bool IsPrimary = false
) : IRequest<Result<string>>;

