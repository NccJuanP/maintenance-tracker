using System.Net;
using System.Text.Json;
using MaintenanceTracker.Api.IntegrationTests.Infrastructure;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.MaintenanceRequests;

[Collection(ApiCollection.Name)]
public sealed class ActivityTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Activity_ListsEveryChangeInChronologicalOrderWithContractShape()
    {
        var id = await CreateWithHistoryAsync();

        var page = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}/{id}/activity");

        page.GetProperty("totalCount").GetInt32().ShouldBe(4);
        var items = page.GetProperty("items").EnumerateArray().ToList();
        items.Select(item => item.GetProperty("type").GetString()).ShouldBe(
            ["Created", "AssigneeChanged", "StatusChanged", "AssigneeChanged"]);

        items[0].GetProperty("performedBy").GetProperty("name").GetString().ShouldBe("Ana Restrepo");
        items[0].GetProperty("occurredAt").GetString().ShouldEndWith("Z");

        items[1].GetProperty("performedBy").GetProperty("name").GetString().ShouldBe("Laura Gómez");
        items[1].GetProperty("previousAssignee").ValueKind.ShouldBe(JsonValueKind.Null);
        items[1].GetProperty("newAssignee").GetProperty("name").GetString().ShouldBe("Carlos Mejía");

        items[2].GetProperty("performedBy").GetProperty("name").GetString().ShouldBe("Carlos Mejía");
        items[2].GetProperty("fromStatus").GetString().ShouldBe("Pending");
        items[2].GetProperty("toStatus").GetString().ShouldBe("InProgress");

        items[3].GetProperty("previousAssignee").GetProperty("name").GetString().ShouldBe("Carlos Mejía");
        items[3].GetProperty("newAssignee").GetProperty("name").GetString().ShouldBe("Andrés Ríos");
    }

    [Fact]
    public async Task Activity_IsPaginated()
    {
        var id = await CreateWithHistoryAsync();

        var secondPage = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}/{id}/activity?page=2&pageSize=3");

        secondPage.GetProperty("totalPages").GetInt32().ShouldBe(2);
        var item = secondPage.GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.GetProperty("newAssignee").GetProperty("name").GetString().ShouldBe("Andrés Ríos");
    }

    [Fact]
    public async Task Activity_OfUnknownRequest_Returns404()
    {
        var response = await _client.GetAsync($"{ApiClient.MaintenanceRequests}/{Guid.NewGuid()}/activity");

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "maintenance_request_not_found");
    }

    [Fact]
    public async Task Activity_WithInvalidPageSize_Returns400()
    {
        var id = await _client.CreateRequestAsync("Solicitud con historial", "Other", "Low");

        var response = await _client.GetAsync($"{ApiClient.MaintenanceRequests}/{id}/activity?pageSize=51");

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
    }

    private async Task<Guid> CreateWithHistoryAsync()
    {
        var id = await _client.CreateRequestAsync("Solicitud con historial", "Equipment", "High");
        var url = $"{ApiClient.MaintenanceRequests}/{id}";

        (await _client.PostAsUserAsync($"{url}/assignments", new { assigneeId = TestUsers.Carlos }, TestUsers.Laura))
            .EnsureSuccessStatusCode();
        (await _client.PostAsUserAsync($"{url}/status-changes", new { targetStatus = "InProgress" }, TestUsers.Carlos))
            .EnsureSuccessStatusCode();
        (await _client.PostAsUserAsync($"{url}/assignments", new { assigneeId = TestUsers.Andres }, TestUsers.Ana))
            .EnsureSuccessStatusCode();

        return id;
    }
}
