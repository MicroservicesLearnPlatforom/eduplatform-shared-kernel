namespace EduPlatform.SharedKernel.Common;

// makes sure all our DB changes in a single request either succeed together or fail together
// wraps DbContext operations so we don't bleed EF Core directly into our handlers
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
