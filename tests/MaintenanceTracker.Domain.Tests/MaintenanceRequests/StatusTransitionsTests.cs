using MaintenanceTracker.Domain.Errors;
using MaintenanceTracker.Domain.MaintenanceRequests;
using Shouldly;

namespace MaintenanceTracker.Domain.Tests.MaintenanceRequests;

public sealed class StatusTransitionsTests
{
    // Every status/assignee combination a request can actually reach.
    public static TheoryData<RequestStatus, bool> ReachableStates() => new()
    {
        { RequestStatus.Pending, false },
        { RequestStatus.Pending, true },
        { RequestStatus.InProgress, true },
        { RequestStatus.OnHold, true },
        { RequestStatus.Resolved, true },
        { RequestStatus.Cancelled, false },
        { RequestStatus.Cancelled, true }
    };

    [Theory]
    [MemberData(nameof(ReachableStates))]
    public void AllowedFrom_MatchesWhatTheAggregateAccepts(RequestStatus from, bool hasAssignee)
    {
        var allowed = StatusTransitions.AllowedFrom(from, hasAssignee);

        foreach (var target in Enum.GetValues<RequestStatus>())
        {
            var request = TestRequests.InStatus(from, hasAssignee);
            var accepted = TryChangeStatus(request, target);

            allowed.Contains(target).ShouldBe(accepted, $"{from} -> {target} (assignee: {hasAssignee})");
        }
    }

    [Fact]
    public void AllowedFrom_PendingWithoutAssignee_ExcludesInProgress()
    {
        StatusTransitions.AllowedFrom(RequestStatus.Pending, hasAssignee: false)
            .ShouldBe([RequestStatus.Cancelled]);
    }

    [Theory]
    [InlineData(RequestStatus.Pending, false)]
    [InlineData(RequestStatus.InProgress, false)]
    [InlineData(RequestStatus.OnHold, false)]
    [InlineData(RequestStatus.Resolved, true)]
    [InlineData(RequestStatus.Cancelled, true)]
    public void IsFinal_OnlyForResolvedAndCancelled(RequestStatus status, bool isFinal)
    {
        StatusTransitions.IsFinal(status).ShouldBe(isFinal);
    }

    private static bool TryChangeStatus(MaintenanceRequest request, RequestStatus target)
    {
        try
        {
            request.ChangeStatus(target, TestRequests.Requester, TestRequests.Later);
            return true;
        }
        catch (DomainException)
        {
            return false;
        }
    }
}
