using System.Text.Json;
using MaintenanceTracker.Api.IntegrationTests.Infrastructure;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.MaintenanceRequests;

[Collection(ApiCollection.Name)]
public sealed class SummaryTests(ApiFactory factory)
{
    private const string SummaryUrl = $"{ApiClient.MaintenanceRequests}/summary";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Summary_AlwaysListsTheFiveStatusesAndTotalIsTheirSum()
    {
        var summary = await _client.GetJsonAsync(SummaryUrl);

        var byStatus = ByStatus(summary);
        byStatus.Keys.ShouldBe(["Pending", "InProgress", "OnHold", "Resolved", "Cancelled"], ignoreOrder: true);
        summary.GetProperty("total").GetInt32().ShouldBe(byStatus.Values.Sum());
    }

    // Other tests share the database, so the assertion is on the change, not on absolute numbers.
    // Tests in the same collection run one at a time, so nothing else writes in between.
    [Fact]
    public async Task Summary_ReflectsNewAndCancelledRequests()
    {
        var before = await _client.GetJsonAsync(SummaryUrl);

        await _client.CreateRequestAsync("Solicitud para el resumen", "Other", "Low");
        var cancelled = await _client.CreateRequestAsync("Solicitud cancelada", "Other", "Low");
        (await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{cancelled}/status-changes", new { targetStatus = "Cancelled" }, TestUsers.Ana))
            .EnsureSuccessStatusCode();

        var after = await _client.GetJsonAsync(SummaryUrl);

        after.GetProperty("total").GetInt32().ShouldBe(before.GetProperty("total").GetInt32() + 2);
        ByStatus(after)["Pending"].ShouldBe(ByStatus(before)["Pending"] + 1);
        ByStatus(after)["Cancelled"].ShouldBe(ByStatus(before)["Cancelled"] + 1);
    }

    private static Dictionary<string, int> ByStatus(JsonElement summary) =>
        summary.GetProperty("byStatus").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32());
}
