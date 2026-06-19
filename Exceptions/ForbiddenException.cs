namespace EduPlatform.SharedKernel.Exceptions;

// standard 403 response
// translates to "we know who you are, but you're not allowed to touch this specific thing"
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message)
        : base(message) { }

    public ForbiddenException(string userId, string resource, object key)
        : base($"User '{userId}' is forbidden from accessing {resource} '{key}'.") { }
}
