using EduPlatform.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduPlatform.SharedKernel.Http;

// -------------------------------------------------------
// ExceptionHandlingMiddleware
// The ONE place that turns exceptions into HTTP responses for every service
// (replaces the per-service GlobalExceptionHandlerMiddleware copies).
//
//   SharedKernel / FluentValidation ValidationException -> 400 + errors per field
//   NotFoundException      -> 404
//   ConflictException      -> 409
//   UnauthorizedException  -> 401
//   ForbiddenException     -> 403
//   service mappings       -> as registered (AddEduPlatformExceptionHandling)
//   other DomainException  -> 400 (business rule violated)
//   anything else          -> 500 (details are logged, never sent to the client)
//
// Response: RFC 7807 problem+json. "error" and "message" repeat "detail" because
// the frontend reads those names from the older per-service formats.
// -------------------------------------------------------
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly ExceptionHandlingOptions _options;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IOptions<ExceptionHandlingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away: nothing to answer and no server error to report.
            _logger.LogInformation("Request {Path} was cancelled by the client.", context.Request.Path);
        }
        catch (Exception exception)
        {
            var problem = ToProblem(exception);

            // Real crashes are errors (alerts); 4xx are normal user mistakes.
            if (problem.Status >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
            else
                _logger.LogWarning("{Status} {Type} on {Path}: {Message}",
                    problem.Status, exception.GetType().Name, context.Request.Path, exception.Message);

            if (context.Response.HasStarted)
            {
                // Headers are already sent, the status can no longer be changed.
                _logger.LogWarning("Response already started, cannot write the error response.");
                throw;
            }

            // No Response.Clear(): it would also drop headers set earlier (e.g. CORS), and the
            // browser would then hide the error from the frontend.
            context.Response.StatusCode = problem.Status;
            await context.Response.WriteAsJsonAsync(
                new ProblemResponse(
                    Type: $"https://httpstatuses.io/{problem.Status}",
                    Title: problem.Title,
                    Status: problem.Status,
                    Detail: problem.Detail,
                    Instance: context.Request.Path,
                    TraceId: context.TraceIdentifier,
                    Errors: problem.Errors,
                    Error: problem.Detail,
                    Message: problem.Detail),
                options: (System.Text.Json.JsonSerializerOptions?)null,
                contentType: "application/problem+json");
        }
    }

    private Problem ToProblem(Exception exception)
    {
        switch (exception)
        {
            case ValidationException validation:
                return Validation(validation.Errors);

            case FluentValidation.ValidationException fluent:
                return Validation(fluent.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

            case NotFoundException:
                return new Problem(StatusCodes.Status404NotFound, "Not Found", exception.Message);
            case ConflictException:
                return new Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message);
            case UnauthorizedException:
                return new Problem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message);
            case ForbiddenException:
                return new Problem(StatusCodes.Status403Forbidden, "Forbidden", exception.Message);
        }

        // Service-specific exceptions (e.g. AiGenerationFailedException -> 503).
        foreach (var mapping in _options.Mappings)
        {
            if (mapping.ExceptionType.IsInstanceOfType(exception))
                return new Problem(mapping.StatusCode, mapping.Title, exception.Message);
        }

        if (exception is DomainException)
            return new Problem(StatusCodes.Status400BadRequest, "Bad Request", exception.Message);

        return new Problem(StatusCodes.Status500InternalServerError, "Internal Server Error",
            "An unexpected error occurred.");
    }

    private static Problem Validation(IDictionary<string, string[]> errors)
    {
        // Readable one-liner for toasts, e.g. "Amount: must be greater than 0; Currency: required".
        var detail = string.Join("; ", errors.SelectMany(kv => kv.Value.Select(msg => $"{kv.Key}: {msg}")));
        return new Problem(StatusCodes.Status400BadRequest, "Validation failed",
            string.IsNullOrEmpty(detail) ? "One or more validation errors occurred." : detail,
            errors);
    }

    private sealed record Problem(int Status, string Title, string Detail, IDictionary<string, string[]>? Errors = null);

    // camelCase via the ASP.NET Core web defaults of WriteAsJsonAsync.
    private sealed record ProblemResponse(
        string Type,
        string Title,
        int Status,
        string Detail,
        string Instance,
        string TraceId,
        [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        IDictionary<string, string[]>? Errors,
        string Error,
        string Message);
}

public sealed class ExceptionHandlingOptions
{
    internal List<ExceptionMapping> Mappings { get; } = new();

    /// <summary>Maps a service-specific exception (and its subclasses) to a status code.</summary>
    public ExceptionHandlingOptions Map<TException>(int statusCode, string? title = null)
        where TException : Exception
    {
        Mappings.Add(new ExceptionMapping(typeof(TException), statusCode, title ?? ReasonPhrase(statusCode)));
        return this;
    }

    private static string ReasonPhrase(int statusCode) =>
        Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(statusCode) is { Length: > 0 } phrase
            ? phrase
            : "Error";
}

internal sealed record ExceptionMapping(Type ExceptionType, int StatusCode, string Title);

public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Optional: register service-specific exception mappings.
    /// <c>services.AddEduPlatformExceptionHandling(o => o.Map&lt;AiGenerationFailedException&gt;(503));</c>
    /// </summary>
    public static IServiceCollection AddEduPlatformExceptionHandling(
        this IServiceCollection services,
        Action<ExceptionHandlingOptions>? configure = null)
    {
        var builder = services.AddOptions<ExceptionHandlingOptions>();
        if (configure is not null)
            builder.Configure(configure);
        return services;
    }

    /// <summary>Put it early in the pipeline so it also covers the middlewares after it.</summary>
    public static IApplicationBuilder UseEduPlatformExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
