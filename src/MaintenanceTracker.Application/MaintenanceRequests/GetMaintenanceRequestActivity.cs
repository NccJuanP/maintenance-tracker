using MaintenanceTracker.Application.Errors;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class GetMaintenanceRequestActivity(IMaintenanceRequestQueries queries)
{
    public async Task<PagedResult<ActivityEntryDto>> ExecuteAsync(
        Guid maintenanceRequestId, int page, int pageSize, CancellationToken cancellationToken) =>
        await queries.GetActivityAsync(maintenanceRequestId, page, pageSize, cancellationToken)
        ?? throw UseCaseException.MaintenanceRequestNotFound(maintenanceRequestId);
}
