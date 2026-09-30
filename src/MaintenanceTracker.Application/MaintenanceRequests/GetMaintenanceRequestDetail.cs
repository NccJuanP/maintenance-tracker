using MaintenanceTracker.Application.Errors;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class GetMaintenanceRequestDetail(IMaintenanceRequestQueries queries)
{
    public async Task<MaintenanceRequestDetail> ExecuteAsync(Guid id, CancellationToken cancellationToken) =>
        await queries.GetDetailAsync(id, cancellationToken)
        ?? throw UseCaseException.MaintenanceRequestNotFound(id);
}
