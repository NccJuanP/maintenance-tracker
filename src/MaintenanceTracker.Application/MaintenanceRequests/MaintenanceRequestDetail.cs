using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed record MaintenanceRequestDetail(
    Guid Id,
    string Title,
    string Description,
    Category Category,
    Priority Priority,
    RequestStatus Status,
    UserSummary? Assignee,
    UserSummary Requester,
    DateTimeOffset CreatedAt,
    IReadOnlyList<RequestStatus> AllowedTransitions,
    bool CanChangeAssignee);
