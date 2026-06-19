namespace EduPlatform.SharedKernel.Domain;

// allows us to 'delete' a record without actually wiping it from the DB
// EF Core intercepts these with a query filter so they just disappear from normal queries automatically
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
