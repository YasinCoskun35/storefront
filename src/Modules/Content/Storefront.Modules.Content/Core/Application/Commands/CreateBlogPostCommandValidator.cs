using FluentValidation;

namespace Storefront.Modules.Content.Core.Application.Commands;

public sealed class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
{
    public CreateBlogPostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");

        RuleFor(x => x.Summary)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Summary))
            .WithMessage("Summary must not exceed 1000 characters.");

        RuleFor(x => x.Author)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Author))
            .WithMessage("Author name must not exceed 200 characters.");

        RuleFor(x => x.Tags)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Tags))
            .WithMessage("Tags must not exceed 500 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("Category must not exceed 200 characters.");
    }
}

