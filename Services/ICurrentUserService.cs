namespace EduPlatform.SharedKernel.Services;

// tells us who is currently calling the API
// each microservice implements this (usually grabbing data from the HttpContext window)
// EF Core uses this to magically fill in the CreatedBy and UpdatedBy fields during saves
public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserRole { get; }
    bool IsAuthenticated { get; }
}
