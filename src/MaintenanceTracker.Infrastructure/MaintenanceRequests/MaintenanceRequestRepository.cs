using MaintenanceTracker.Application.MaintenanceRequests;
using MaintenanceTracker.Domain.MaintenanceRequests;
using MaintenanceTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceTracker.Infrastructure.MaintenanceRequests;

internal sealed class MaintenanceRequestRepository(MaintenanceTrackerDbContext dbContext) : IMaintenanceRequestRepository
{
    // The history is intentionally not loaded: no rule depends on past entries. After loading,
    // ActivityEntries only contains the entries added by the current command, which EF Core
    // inserts in the same SaveChanges as the aggregate change.
    public Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.MaintenanceRequests.SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    public void Add(MaintenanceRequest maintenanceRequest) =>
        dbContext.MaintenanceRequests.Add(maintenanceRequest);
}
