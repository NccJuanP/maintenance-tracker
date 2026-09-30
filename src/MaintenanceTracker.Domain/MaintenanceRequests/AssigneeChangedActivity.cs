namespace MaintenanceTracker.Domain.MaintenanceRequests;

public sealed class AssigneeChangedActivity : ActivityEntry
{
    internal AssigneeChangedActivity(
        Guid id,
        Guid maintenanceRequestId,
        DateTimeOffset occurredAt,
        Guid performedById,
        Guid? previousAssigneeId,
        Guid newAssigneeId)
        : base(id, maintenanceRequestId, occurredAt, performedById)
    {
        PreviousAssigneeId = previousAssigneeId;
        NewAssigneeId = newAssigneeId;
    }

    public Guid? PreviousAssigneeId { get; private set; }
    public Guid NewAssigneeId { get; private set; }
}
