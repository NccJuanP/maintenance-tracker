using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public enum SortDirection
{
    Asc,
    Desc
}

public sealed record MaintenanceRequestListQuery(
    int Page,
    int PageSize,
    RequestStatus? Status,
    Priority? Priority,
    Category? Category,
    string? Search,
    SortDirection SortDirection);
