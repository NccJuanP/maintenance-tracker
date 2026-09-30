namespace MaintenanceTracker.Application.MaintenanceRequests;

public interface IMaintenanceRequestQueries
{
    Task<MaintenanceRequestDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
}
