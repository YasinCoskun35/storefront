using FluentValidation;
using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before handlers.
/// Converts validation failures into Result.Failure / Result&lt;T&gt;.Failure.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var message = string.Join("; ", failures.Select(f => f.ErrorMessage));
        var error = Error.Validation("Validation.Failed", message);

        var responseType = typeof(TResponse);

        // Handle Result<T>
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(
                "Failure",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                [typeof(Error)]);

            if (failureMethod is not null)
                return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        // Handle non-generic Result
        if (responseType == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        return await next();
    }
}
