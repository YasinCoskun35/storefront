using FluentValidation;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Slug)
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters")
            .Matches(@"^[a-z0-9-]*$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}



