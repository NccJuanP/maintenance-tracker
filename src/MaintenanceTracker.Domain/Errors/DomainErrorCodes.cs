namespace MaintenanceTracker.Domain.Errors;

public static class DomainErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string InvalidStatusTransition = "invalid_status_transition";
    public const string AssigneeRequired = "assignee_required";
    public const string RequestClosed = "request_closed";
    public const string AssigneeUnchanged = "assignee_unchanged";
}
