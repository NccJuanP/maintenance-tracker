using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MaintenanceTracker.Api.IntegrationTests.Infrastructure;
using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.MaintenanceRequests;

[Collection(ApiCollection.Name)]
public sealed class WriteFlowTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateAssignAndStart_PersistsStateAndHistoryInOrder()
    {
        var createResponse = await _client.PostAsUserAsync(
            ApiClient.MaintenanceRequests, ApiClient.ValidCreateBody(), TestUsers.Ana);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await createResponse.ReadDetailAsync();
        createResponse.Headers.Location!.AbsolutePath.ShouldBe($"{ApiClient.MaintenanceRequests}/{created.Id}");
        created.Status.ShouldBe(RequestStatus.Pending);
        created.Requester.ShouldBe(new UserSummary(TestUsers.Ana, "Ana Restrepo"));
        created.Assignee.ShouldBeNull();
        created.AllowedTransitions.ShouldBe([RequestStatus.Cancelled]);
        created.CanChangeAssignee.ShouldBeTrue();

        var assignResponse = await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{created.Id}/assignments", new { assigneeId = TestUsers.Carlos }, TestUsers.Laura);

        assignResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var assigned = await assignResponse.ReadDetailAsync();
        assigned.Assignee.ShouldBe(new UserSummary(TestUsers.Carlos, "Carlos Mejía"));
        assigned.AllowedTransitions.ShouldBe([RequestStatus.InProgress, RequestStatus.Cancelled]);

        var startResponse = await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{created.Id}/status-changes", new { targetStatus = "InProgress" }, TestUsers.Carlos);

        startResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var started = await startResponse.ReadDetailAsync();
        started.Status.ShouldBe(RequestStatus.InProgress);
        started.AllowedTransitions.ShouldBe([RequestStatus.OnHold, RequestStatus.Resolved, RequestStatus.Cancelled]);

        var stored = await (await _client.GetAsync($"{ApiClient.MaintenanceRequests}/{created.Id}")).ReadDetailAsync();
        stored.Status.ShouldBe(RequestStatus.InProgress);
        stored.Assignee.ShouldBe(new UserSummary(TestUsers.Carlos, "Carlos Mejía"));

        var activity = await factory.GetStoredActivityAsync(created.Id);
        activity.Count.ShouldBe(3);

        var createdEntry = activity[0].ShouldBeOfType<CreatedActivity>();
        createdEntry.PerformedById.ShouldBe(TestUsers.Ana);

        var assignedEntry = activity[1].ShouldBeOfType<AssigneeChangedActivity>();
        assignedEntry.PerformedById.ShouldBe(TestUsers.Laura);
        assignedEntry.PreviousAssigneeId.ShouldBeNull();
        assignedEntry.NewAssigneeId.ShouldBe(TestUsers.Carlos);

        var startedEntry = activity[2].ShouldBeOfType<StatusChangedActivity>();
        startedEntry.PerformedById.ShouldBe(TestUsers.Carlos);
        startedEntry.FromStatus.ShouldBe(RequestStatus.Pending);
        startedEntry.ToStatus.ShouldBe(RequestStatus.InProgress);
    }

    [Fact]
    public async Task Create_ReturnsContractShape()
    {
        var response = await _client.PostAsUserAsync(
            ApiClient.MaintenanceRequests, ApiClient.ValidCreateBody(), TestUsers.Ana);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("status").GetString().ShouldBe("Pending");
        json.GetProperty("category").GetString().ShouldBe("Equipment");
        json.GetProperty("assignee").ValueKind.ShouldBe(JsonValueKind.Null);
        json.GetProperty("requester").GetProperty("name").GetString().ShouldBe("Ana Restrepo");
        json.GetProperty("allowedTransitions")[0].GetString().ShouldBe("Cancelled");
        json.GetProperty("createdAt").GetString().ShouldEndWith("Z");
    }

    [Fact]
    public async Task InvalidTransition_Returns409AndDoesNotRecordHistory()
    {
        var created = await _client.CreateRequestAsync(TestUsers.Ana);

        var response = await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{created.Id}/status-changes", new { targetStatus = "Resolved" }, TestUsers.Ana);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "invalid_status_transition");
        (await factory.GetStoredActivityAsync(created.Id)).ShouldHaveSingleItem().ShouldBeOfType<CreatedActivity>();
        var stored = await (await _client.GetAsync($"{ApiClient.MaintenanceRequests}/{created.Id}")).ReadDetailAsync();
        stored.Status.ShouldBe(RequestStatus.Pending);
    }

    [Fact]
    public async Task StartWithoutAssignee_Returns409AssigneeRequired()
    {
        var created = await _client.CreateRequestAsync(TestUsers.Ana);

        var response = await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{created.Id}/status-changes", new { targetStatus = "InProgress" }, TestUsers.Ana);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "assignee_required");
    }

    [Fact]
    public async Task AssignSameAssignee_Returns409AssigneeUnchanged()
    {
        var created = await _client.CreateRequestAsync(TestUsers.Ana);
        var url = $"{ApiClient.MaintenanceRequests}/{created.Id}/assignments";
        (await _client.PostAsUserAsync(url, new { assigneeId = TestUsers.Carlos }, TestUsers.Ana)).EnsureSuccessStatusCode();

        var response = await _client.PostAsUserAsync(url, new { assigneeId = TestUsers.Carlos }, TestUsers.Ana);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "assignee_unchanged");
        (await factory.GetStoredActivityAsync(created.Id)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task AssignUnknownAssignee_Returns400AssigneeNotFound()
    {
        var created = await _client.CreateRequestAsync(TestUsers.Ana);

        var response = await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{created.Id}/assignments", new { assigneeId = TestUsers.Unknown }, TestUsers.Ana);

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "assignee_not_found");
    }

    [Fact]
    public async Task UnknownRequest_Returns404()
    {
        var unknownId = Guid.NewGuid();

        var getResponse = await _client.GetAsync($"{ApiClient.MaintenanceRequests}/{unknownId}");
        var assignResponse = await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{unknownId}/assignments", new { assigneeId = TestUsers.Carlos }, TestUsers.Ana);

        await getResponse.ShouldBeProblemAsync(HttpStatusCode.NotFound, "maintenance_request_not_found");
        await assignResponse.ShouldBeProblemAsync(HttpStatusCode.NotFound, "maintenance_request_not_found");
    }
}
