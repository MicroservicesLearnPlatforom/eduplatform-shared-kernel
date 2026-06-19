using Microsoft.Extensions.Configuration;

namespace EduPlatform.SharedKernel.Http;

// -------------------------------------------------------
// InternalAuthHandler — delegating handler that appends a shared
// internal API key to every outbound service-to-service HTTP request.
// Why: without this, any process on the internal network can call
//      service endpoints without authentication.
// How: registered as a DelegatingHandler on typed/named HttpClients.
//      The receiving service validates the header in its middleware
//      for any route under /api/internal/*.
// -------------------------------------------------------
public class InternalAuthHandler : DelegatingHandler
{
    private readonly string _apiKey;

    public InternalAuthHandler(IConfiguration configuration)
    {
        _apiKey = configuration["InternalApi:Key"]
            ?? throw new InvalidOperationException(
                "Missing configuration key 'InternalApi:Key'. " +
                "Set the INTERNALAPI__KEY environment variable.");
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Attach key on every outbound call so receiving services can
        // verify the request originated from a trusted internal service.
        request.Headers.TryAddWithoutValidation("X-Internal-Key", _apiKey);

        // Forward correlation ID if already present in the current context.
        if (CorrelationContext.Current is { } correlationId)
            request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}
