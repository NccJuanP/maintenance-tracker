using MaintenanceTracker.Application;
using MaintenanceTracker.Application.Errors;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceTracker.Infrastructure.Persistence;

internal sealed class UnitOfWork(MaintenanceTrackerDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw UseCaseException.ConcurrencyConflict();
        }
    }
}
