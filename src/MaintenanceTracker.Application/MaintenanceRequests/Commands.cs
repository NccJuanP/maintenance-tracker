using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed record CreateMaintenanceRequestCommand(
    string Title,
    string Description,
    Category Category,
    Priority Priority,
    Guid PerformedById);

public sealed record AssignMaintenanceRequestCommand(
    Guid MaintenanceRequestId,
    Guid AssigneeId,
    Guid PerformedById);

public sealed record ChangeMaintenanceRequestStatusCommand(
    Guid MaintenanceRequestId,
    RequestStatus TargetStatus,
    Guid PerformedById);
