namespace EduPlatform.SharedKernel.Http;

// -------------------------------------------------------
// CorrelationContext — AsyncLocal storage for the current
// correlation / trace ID that flows through a single request.
// Why: allows middleware and HTTP handlers to share the same
//      correlation ID without constructor injection everywhere.
// -------------------------------------------------------
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();

    public static string? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
