namespace MaintenanceTracker.Application.MaintenanceRequests;

public interface IMaintenanceRequestQueries
{
    Task<MaintenanceRequestDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<MaintenanceRequestListItem>> ListAsync(
        MaintenanceRequestListQuery query, CancellationToken cancellationToken);

    // Null when the maintenance request does not exist.
    Task<PagedResult<ActivityEntryDto>?> GetActivityAsync(
        Guid maintenanceRequestId, int page, int pageSize, CancellationToken cancellationToken);

    Task<RequestSummary> GetSummaryAsync(CancellationToken cancellationToken);
}
