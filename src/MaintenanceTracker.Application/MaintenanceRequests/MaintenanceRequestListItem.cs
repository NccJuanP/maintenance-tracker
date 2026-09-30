using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed record MaintenanceRequestListItem(
    Guid Id,
    string Title,
    Category Category,
    Priority Priority,
    RequestStatus Status,
    UserSummary? Assignee,
    DateTimeOffset CreatedAt);
