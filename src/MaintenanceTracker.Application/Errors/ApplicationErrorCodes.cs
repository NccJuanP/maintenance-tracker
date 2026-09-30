namespace MaintenanceTracker.Application.Errors;

public static class ApplicationErrorCodes
{
    public const string UserNotFound = "user_not_found";
    public const string AssigneeNotFound = "assignee_not_found";
    public const string MaintenanceRequestNotFound = "maintenance_request_not_found";
    public const string ConcurrencyConflict = "concurrency_conflict";
}
