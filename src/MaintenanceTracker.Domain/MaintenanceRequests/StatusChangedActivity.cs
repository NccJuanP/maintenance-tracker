namespace MaintenanceTracker.Domain.MaintenanceRequests;

public sealed class StatusChangedActivity : ActivityEntry
{
    internal StatusChangedActivity(
        Guid id,
        Guid maintenanceRequestId,
        DateTimeOffset occurredAt,
        Guid performedById,
        RequestStatus fromStatus,
        RequestStatus toStatus)
        : base(id, maintenanceRequestId, occurredAt, performedById)
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }

    public RequestStatus FromStatus { get; private set; }
    public RequestStatus ToStatus { get; private set; }
}
