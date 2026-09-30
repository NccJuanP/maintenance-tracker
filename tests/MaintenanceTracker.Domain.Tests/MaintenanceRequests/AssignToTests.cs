using MaintenanceTracker.Domain.Errors;
using MaintenanceTracker.Domain.MaintenanceRequests;
using Shouldly;

namespace MaintenanceTracker.Domain.Tests.MaintenanceRequests;

public sealed class AssignToTests
{
    [Fact]
    public void AssignTo_WithoutPreviousAssignee_RecordsEntryWithNullPrevious()
    {
        var request = TestRequests.CreatePending();

        request.AssignTo(TestRequests.Assignee, TestRequests.Requester, TestRequests.Later);

        request.AssigneeId.ShouldBe(TestRequests.Assignee);
        request.ActivityEntries.Count.ShouldBe(2);
        var entry = request.ActivityEntries.Last().ShouldBeOfType<AssigneeChangedActivity>();
        entry.PreviousAssigneeId.ShouldBeNull();
        entry.NewAssigneeId.ShouldBe(TestRequests.Assignee);
        entry.PerformedById.ShouldBe(TestRequests.Requester);
        entry.OccurredAt.ShouldBe(TestRequests.Later);
        entry.MaintenanceRequestId.ShouldBe(request.Id);
    }

    [Theory]
    [InlineData(RequestStatus.Pending)]
    [InlineData(RequestStatus.InProgress)]
    [InlineData(RequestStatus.OnHold)]
    public void AssignTo_OtherUserInNonFinalStatus_ReassignsAndRecordsPrevious(RequestStatus status)
    {
        var request = TestRequests.InStatus(status);
        var entriesBefore = request.ActivityEntries.Count;

        request.AssignTo(TestRequests.OtherAssignee, TestRequests.Requester, TestRequests.Later);

        request.AssigneeId.ShouldBe(TestRequests.OtherAssignee);
        request.Status.ShouldBe(status);
        request.ActivityEntries.Count.ShouldBe(entriesBefore + 1);
        var entry = request.ActivityEntries.Last().ShouldBeOfType<AssigneeChangedActivity>();
        entry.PreviousAssigneeId.ShouldBe(TestRequests.Assignee);
        entry.NewAssigneeId.ShouldBe(TestRequests.OtherAssignee);
    }

    [Theory]
    [InlineData(RequestStatus.Resolved)]
    [InlineData(RequestStatus.Cancelled)]
    public void AssignTo_InFinalStatus_ThrowsRequestClosed(RequestStatus status)
    {
        var request = TestRequests.InStatus(status);
        var entriesBefore = request.ActivityEntries.Count;

        var exception = Should.Throw<DomainException>(
            () => request.AssignTo(TestRequests.OtherAssignee, TestRequests.Requester, TestRequests.Later));

        exception.Code.ShouldBe(DomainErrorCodes.RequestClosed);
        request.AssigneeId.ShouldBe(TestRequests.Assignee);
        request.ActivityEntries.Count.ShouldBe(entriesBefore);
    }

    [Fact]
    public void AssignTo_SameAssignee_ThrowsAssigneeUnchanged()
    {
        var request = TestRequests.InStatus(RequestStatus.InProgress);
        var entriesBefore = request.ActivityEntries.Count;

        var exception = Should.Throw<DomainException>(
            () => request.AssignTo(TestRequests.Assignee, TestRequests.Requester, TestRequests.Later));

        exception.Code.ShouldBe(DomainErrorCodes.AssigneeUnchanged);
        request.ActivityEntries.Count.ShouldBe(entriesBefore);
    }

    [Fact]
    public void AssignTo_ChecksClosedBeforeUnchanged()
    {
        var request = TestRequests.InStatus(RequestStatus.Resolved);

        var exception = Should.Throw<DomainException>(
            () => request.AssignTo(TestRequests.Assignee, TestRequests.Requester, TestRequests.Later));

        exception.Code.ShouldBe(DomainErrorCodes.RequestClosed);
    }
}
