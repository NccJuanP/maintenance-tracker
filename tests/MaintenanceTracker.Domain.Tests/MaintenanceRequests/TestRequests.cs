using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Domain.Tests.MaintenanceRequests;

internal static class TestRequests
{
    public static readonly Guid Requester = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b01");
    public static readonly Guid Assignee = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b03");
    public static readonly Guid OtherAssignee = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b04");
    public static readonly DateTimeOffset CreatedAt = new(2026, 9, 30, 13, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Later = CreatedAt.AddMinutes(30);

    public static MaintenanceRequest CreatePending() =>
        MaintenanceRequest.Create(
            "Aire acondicionado sin enfriar",
            "El equipo de la sala 2 no responde al control.",
            Category.Equipment,
            Priority.High,
            Requester,
            CreatedAt);

    // Reaches the status only through valid transitions, so the setup never bypasses the rules under test.
    public static MaintenanceRequest InStatus(RequestStatus status, bool withAssignee = true)
    {
        var request = CreatePending();

        if (withAssignee)
        {
            request.AssignTo(Assignee, Requester, CreatedAt);
        }

        RequestStatus[] path = status switch
        {
            RequestStatus.Pending => [],
            RequestStatus.InProgress => [RequestStatus.InProgress],
            RequestStatus.OnHold => [RequestStatus.InProgress, RequestStatus.OnHold],
            RequestStatus.Resolved => [RequestStatus.InProgress, RequestStatus.Resolved],
            RequestStatus.Cancelled => [RequestStatus.Cancelled],
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };

        foreach (var step in path)
        {
            request.ChangeStatus(step, Requester, CreatedAt);
        }

        return request;
    }
}
