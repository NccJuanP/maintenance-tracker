namespace MaintenanceTracker.Domain.MaintenanceRequests;

public abstract class ActivityEntry
{
    protected ActivityEntry(Guid id, Guid maintenanceRequestId, DateTimeOffset occurredAt, Guid performedById)
    {
        Id = id;
        MaintenanceRequestId = maintenanceRequestId;
        OccurredAt = occurredAt;
        PerformedById = performedById;
    }

    public Guid Id { get; private set; }
    public Guid MaintenanceRequestId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public Guid PerformedById { get; private set; }
}
