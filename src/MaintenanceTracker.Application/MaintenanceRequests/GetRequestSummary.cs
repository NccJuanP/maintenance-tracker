namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class GetRequestSummary(IMaintenanceRequestQueries queries)
{
    public Task<RequestSummary> ExecuteAsync(CancellationToken cancellationToken) =>
        queries.GetSummaryAsync(cancellationToken);
}
