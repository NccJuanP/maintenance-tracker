using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed record RequestSummary(int Total, IReadOnlyDictionary<RequestStatus, int> ByStatus);
