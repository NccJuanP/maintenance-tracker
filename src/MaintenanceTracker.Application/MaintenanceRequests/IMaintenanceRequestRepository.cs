using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public interface IMaintenanceRequestRepository
{
    Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(MaintenanceRequest maintenanceRequest);
}
