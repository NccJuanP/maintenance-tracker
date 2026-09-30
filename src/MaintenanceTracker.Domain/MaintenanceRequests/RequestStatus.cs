namespace MaintenanceTracker.Domain.MaintenanceRequests;

public enum RequestStatus
{
    Pending,
    InProgress,
    OnHold,
    Resolved,
    Cancelled
}
