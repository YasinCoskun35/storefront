using FluentValidation;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.PartnerUserId)
            .NotEmpty().WithMessage("Partner user ID is required");

        RuleFor(x => x.PartnerCompanyId)
            .NotEmpty().WithMessage("Partner company ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(500);

        RuleFor(x => x.ProductSKU)
            .NotEmpty().WithMessage("Product SKU is required")
            .MaximumLength(100);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10,000");

        RuleFor(x => x.CustomizationNotes)
            .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.CustomizationNotes));
    }
}
