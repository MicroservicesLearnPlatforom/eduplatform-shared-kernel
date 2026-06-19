namespace EduPlatform.SharedKernel.Exceptions;

// standard 401 unauthenticated response wrapper
// basically "we don't know who you are, or your token expired"
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message)
        : base(message) { }

    public UnauthorizedException(string action, string resource, object key)
        : base($"Unauthorized to {action} {resource} '{key}'.") { }
}
