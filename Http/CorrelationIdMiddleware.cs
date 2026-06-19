using Microsoft.AspNetCore.Http;

namespace EduPlatform.SharedKernel.Http;

// -------------------------------------------------------
// CorrelationIdMiddleware — generates or forwards a unique
// X-Correlation-ID header for every inbound HTTP request.
// Why: without a stable trace ID, correlating logs across
//      multiple microservices is extremely difficult.
// How: placed near the top of the middleware pipeline; stores
//      the ID in CorrelationContext so outbound HTTP clients
//      (via InternalAuthHandler) can forward it downstream.
// -------------------------------------------------------
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Reuse the incoming correlation ID (from gateway/client) or
        // generate a new one so every request has a stable trace ID.
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        CorrelationContext.Current = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);

        // Clear so the AsyncLocal doesn't bleed into thread-pool reuse.
        CorrelationContext.Current = null;
    }
}
