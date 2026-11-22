using System.Text.Json;
using EduPlatform.SharedKernel.Behaviors;
using EduPlatform.SharedKernel.Exceptions;
using EduPlatform.SharedKernel.Http;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using ValidationException = EduPlatform.SharedKernel.Exceptions.ValidationException;

namespace EduPlatform.SharedKernel.Tests;

public class ExceptionHandlingMiddlewareTests
{
    // ---- the bug this replaces: invalid input must be 400, never 500 ----

    [Fact]
    public async Task Invalid_request_through_ValidationBehavior_returns_400_with_field_errors()
    {
        var (status, body) = await Run(async () =>
        {
            var behavior = new ValidationBehavior<Pay, string>(new IValidator<Pay>[] { new PayValidator() });
            await behavior.Handle(new Pay(-5, ""), () => Task.FromResult("handler"), default);
        });

        Assert.Equal(400, status);
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Amount", out _));
        Assert.True(errors.TryGetProperty("Currency", out _));
        Assert.Contains("Amount:", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task FluentValidation_exception_is_also_400_with_grouped_errors()
    {
        var (status, body) = await Run(() => throw new FluentValidation.ValidationException(new[]
        {
            new FluentValidation.Results.ValidationFailure("Email", "required"),
            new FluentValidation.Results.ValidationFailure("Email", "invalid"),
        }));

        Assert.Equal(400, status);
        Assert.Equal(2, body.GetProperty("errors").GetProperty("Email").GetArrayLength());
    }

    // ---- status mapping ----

    public static TheoryData<Exception, int> StatusCases => new()
    {
        { new NotFoundException("Course", Guid.Empty), 404 },
        { new ConflictException("boom"), 409 },
        { new UnauthorizedException("boom"), 401 },
        { new ForbiddenException("boom"), 403 },
        { new DomainException("boom"), 400 },
        { new InvalidOperationException("boom"), 500 },
    };

    [Theory]
    [MemberData(nameof(StatusCases))]
    public async Task Maps_exception_types_to_status_codes(Exception exception, int expected)
    {
        var (status, _) = await Run(() => throw exception);

        Assert.Equal(expected, status);
    }

    [Fact]
    public async Task Server_errors_do_not_leak_the_exception_message()
    {
        var (status, body) = await Run(() => throw new InvalidOperationException("connection string secret"));

        Assert.Equal(500, status);
        Assert.DoesNotContain("secret", body.GetRawText());
    }

    [Fact]
    public async Task Service_specific_mapping_wins_over_DomainException()
    {
        var options = new ExceptionHandlingOptions().Map<ServiceUnavailableDomainException>(503);

        var (status, body) = await Run(() => throw new ServiceUnavailableDomainException("AI down"), options);

        Assert.Equal(503, status);
        Assert.Equal("AI down", body.GetProperty("detail").GetString());
    }

    // ---- response format the frontend reads ----

    [Fact]
    public async Task Response_is_problem_json_with_frontend_compatible_fields()
    {
        var context = NewContext();
        var exception = new NotFoundException("Course", Guid.Empty);
        await Middleware(() => throw exception).InvokeAsync(context);

        Assert.Equal("application/problem+json", context.Response.ContentType);
        var body = ReadBody(context);
        foreach (var field in new[] { "detail", "error", "message" })
            Assert.Equal(exception.Message, body.GetProperty(field).GetString());
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.False(body.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Headers_set_before_the_exception_are_kept()
    {
        var context = NewContext();
        await Middleware(() =>
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "https://ahashift.com";
            throw new ConflictException("taken");
        }).InvokeAsync(context);

        Assert.Equal(409, context.Response.StatusCode);
        Assert.Equal("https://ahashift.com", context.Response.Headers["Access-Control-Allow-Origin"].ToString());
    }

    [Fact]
    public async Task Successful_request_passes_through_untouched()
    {
        var (status, _) = await Run(() => Task.CompletedTask, readBody: false);

        Assert.Equal(200, status);
    }

    [Fact]
    public async Task Request_cancelled_by_the_client_is_not_turned_into_a_500()
    {
        var context = NewContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        context.RequestAborted = cts.Token;

        await Middleware(() => throw new OperationCanceledException()).InvokeAsync(context);

        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    // ---- helpers ----

    private static async Task<(int Status, JsonElement Body)> Run(
        Func<Task> next, ExceptionHandlingOptions? options = null, bool readBody = true)
    {
        var context = NewContext();
        await Middleware(next, options).InvokeAsync(context);
        return (context.Response.StatusCode, readBody ? ReadBody(context) : default);
    }

    private static ExceptionHandlingMiddleware Middleware(Func<Task> next, ExceptionHandlingOptions? options = null) =>
        new(_ => next(), NullLogger<ExceptionHandlingMiddleware>.Instance,
            Options.Create(options ?? new ExceptionHandlingOptions()));

    private static DefaultHttpContext NewContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static JsonElement ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return JsonDocument.Parse(context.Response.Body).RootElement.Clone();
    }

    public record Pay(decimal Amount, string Currency) : IRequest<string>;

    private sealed class PayValidator : AbstractValidator<Pay>
    {
        public PayValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty();
        }
    }

    private sealed class ServiceUnavailableDomainException(string message) : DomainException(message);
}
