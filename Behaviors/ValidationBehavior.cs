using FluentValidation;
using MediatR;

namespace EduPlatform.SharedKernel.Behaviors;

// -------------------------------------------------------
// ValidationBehavior<TRequest, TResponse>
// MediatR open-generic pipeline behavior that runs all
// registered FluentValidation IValidator<TRequest>
// implementations before the command/query handler.
//
// Why: centralises input validation so handlers never
//      receive invalid data; throws ValidationException
//      which the exception handler converts to HTTP 400.
// -------------------------------------------------------
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
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
        // Short-circuit immediately if no validators are registered
        // for this request type — avoids unnecessary overhead.
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
