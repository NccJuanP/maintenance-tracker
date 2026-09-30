namespace MaintenanceTracker.Domain.MaintenanceRequests;

public sealed class CreatedActivity : ActivityEntry
{
    internal CreatedActivity(Guid id, Guid maintenanceRequestId, DateTimeOffset occurredAt, Guid performedById)
        : base(id, maintenanceRequestId, occurredAt, performedById)
    {
    }
}
