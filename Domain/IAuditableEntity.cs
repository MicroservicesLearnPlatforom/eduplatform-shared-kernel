namespace EduPlatform.SharedKernel.Domain;

// tracks who created or changed a row and when
// we wire this up in the DbContext SaveChanges to fill automatically, so devs don't have to remember to do it
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
