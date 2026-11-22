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
//      receive invalid data; throws the SharedKernel
//      ValidationException (errors grouped per field), which
//      the services' exception handlers convert to HTTP 400.
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

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        if (errors.Count > 0)
            throw new EduPlatform.SharedKernel.Exceptions.ValidationException(errors);

        return await next();
    }
}
