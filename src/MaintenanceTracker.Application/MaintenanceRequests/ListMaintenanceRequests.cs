namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class ListMaintenanceRequests(IMaintenanceRequestQueries queries)
{
    public Task<PagedResult<MaintenanceRequestListItem>> ExecuteAsync(
        MaintenanceRequestListQuery query, CancellationToken cancellationToken) =>
        queries.ListAsync(query, cancellationToken);
}
