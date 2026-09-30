namespace MaintenanceTracker.Domain.MaintenanceRequests;

public static class StatusTransitions
{
    private static readonly IReadOnlyDictionary<RequestStatus, RequestStatus[]> Table =
        new Dictionary<RequestStatus, RequestStatus[]>
        {
            [RequestStatus.Pending] = [RequestStatus.InProgress, RequestStatus.Cancelled],
            [RequestStatus.InProgress] = [RequestStatus.OnHold, RequestStatus.Resolved, RequestStatus.Cancelled],
            [RequestStatus.OnHold] = [RequestStatus.InProgress, RequestStatus.Cancelled],
            [RequestStatus.Resolved] = [],
            [RequestStatus.Cancelled] = []
        };

    public static bool IsFinal(RequestStatus status) =>
        status is RequestStatus.Resolved or RequestStatus.Cancelled;

    public static bool IsAllowed(RequestStatus from, RequestStatus to) =>
        Table.TryGetValue(from, out var targets) && targets.Contains(to);

    public static bool RequiresAssignee(RequestStatus to) => to == RequestStatus.InProgress;

    public static IReadOnlyList<RequestStatus> AllowedFrom(RequestStatus from, bool hasAssignee) =>
        Table.TryGetValue(from, out var targets)
            ? targets.Where(to => hasAssignee || !RequiresAssignee(to)).ToList()
            : [];
}
