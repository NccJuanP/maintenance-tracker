using MaintenanceTracker.Domain.Errors;
using MaintenanceTracker.Domain.MaintenanceRequests;
using Shouldly;

namespace MaintenanceTracker.Domain.Tests.MaintenanceRequests;

public sealed class ChangeStatusTests
{
    // Written out on purpose instead of reading StatusTransitions: the test must not trust the table it verifies.
    private static readonly HashSet<(RequestStatus From, RequestStatus To)> Allowed =
    [
        (RequestStatus.Pending, RequestStatus.InProgress),
        (RequestStatus.Pending, RequestStatus.Cancelled),
        (RequestStatus.InProgress, RequestStatus.OnHold),
        (RequestStatus.InProgress, RequestStatus.Resolved),
        (RequestStatus.InProgress, RequestStatus.Cancelled),
        (RequestStatus.OnHold, RequestStatus.InProgress),
        (RequestStatus.OnHold, RequestStatus.Cancelled)
    ];

    public static TheoryData<RequestStatus, RequestStatus> AllStatusPairs()
    {
        var data = new TheoryData<RequestStatus, RequestStatus>();
        foreach (var from in Enum.GetValues<RequestStatus>())
        {
            foreach (var to in Enum.GetValues<RequestStatus>())
            {
                data.Add(from, to);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllStatusPairs))]
    public void ChangeStatus_FollowsTransitionTable(RequestStatus from, RequestStatus to)
    {
        var request = TestRequests.InStatus(from);
        var entriesBefore = request.ActivityEntries.Count;

        if (Allowed.Contains((from, to)))
        {
            request.ChangeStatus(to, TestRequests.Assignee, TestRequests.Later);

            request.Status.ShouldBe(to);
            request.ActivityEntries.Count.ShouldBe(entriesBefore + 1);
            var entry = request.ActivityEntries.Last().ShouldBeOfType<StatusChangedActivity>();
            entry.FromStatus.ShouldBe(from);
            entry.ToStatus.ShouldBe(to);
            entry.PerformedById.ShouldBe(TestRequests.Assignee);
            entry.OccurredAt.ShouldBe(TestRequests.Later);
            entry.MaintenanceRequestId.ShouldBe(request.Id);
        }
        else
        {
            var exception = Should.Throw<DomainException>(
                () => request.ChangeStatus(to, TestRequests.Assignee, TestRequests.Later));

            exception.Code.ShouldBe(DomainErrorCodes.InvalidStatusTransition);
            request.Status.ShouldBe(from);
            request.ActivityEntries.Count.ShouldBe(entriesBefore);
        }
    }

    [Fact]
    public void ChangeStatus_ToInProgressWithoutAssignee_ThrowsAssigneeRequired()
    {
        var request = TestRequests.InStatus(RequestStatus.Pending, withAssignee: false);

        var exception = Should.Throw<DomainException>(
            () => request.ChangeStatus(RequestStatus.InProgress, TestRequests.Requester, TestRequests.Later));

        exception.Code.ShouldBe(DomainErrorCodes.AssigneeRequired);
        request.Status.ShouldBe(RequestStatus.Pending);
        request.ActivityEntries.ShouldHaveSingleItem().ShouldBeOfType<CreatedActivity>();
    }

    [Fact]
    public void ChangeStatus_ChecksTransitionTableBeforeAssigneeGuard()
    {
        var request = TestRequests.InStatus(RequestStatus.Cancelled, withAssignee: false);

        var exception = Should.Throw<DomainException>(
            () => request.ChangeStatus(RequestStatus.InProgress, TestRequests.Requester, TestRequests.Later));

        exception.Code.ShouldBe(DomainErrorCodes.InvalidStatusTransition);
    }

    [Fact]
    public void ChangeStatus_ToCancelledWithoutAssignee_IsAllowed()
    {
        var request = TestRequests.InStatus(RequestStatus.Pending, withAssignee: false);

        request.ChangeStatus(RequestStatus.Cancelled, TestRequests.Requester, TestRequests.Later);

        request.Status.ShouldBe(RequestStatus.Cancelled);
    }

    [Fact]
    public void ChangeStatus_WithUndefinedTarget_ThrowsValidation()
    {
        var request = TestRequests.CreatePending();

        var exception = Should.Throw<DomainValidationException>(
            () => request.ChangeStatus((RequestStatus)99, TestRequests.Requester, TestRequests.Later));

        exception.Field.ShouldBe("targetStatus");
        request.ActivityEntries.Count.ShouldBe(1);
    }
}
