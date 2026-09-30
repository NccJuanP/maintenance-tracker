using System.Net;
using System.Text.Json;
using MaintenanceTracker.Api.IntegrationTests.Infrastructure;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.MaintenanceRequests;

[Collection(ApiCollection.Name)]
public sealed class ListTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // The database is shared by all tests, so each test searches by its own token.
    private readonly string _token = Guid.NewGuid().ToString("N")[..10];

    [Fact]
    public async Task Search_IsPartialCaseInsensitiveAndSortedByCreationDescendingByDefault()
    {
        var first = await _client.CreateRequestAsync($"Aire {_token} uno", "Equipment", "High");
        var second = await _client.CreateRequestAsync($"Luz {_token} dos", "Infrastructure", "High");
        var third = await _client.CreateRequestAsync($"Red {_token} tres", "Software", "Low");

        var descending = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token.ToUpperInvariant()}");
        var ascending = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}&sortDirection=asc");

        descending.GetProperty("totalCount").GetInt32().ShouldBe(3);
        Ids(descending).ShouldBe([third, second, first]);
        Ids(ascending).ShouldBe([first, second, third]);
    }

    [Fact]
    public async Task Filters_CombineWithEachOther()
    {
        await _client.CreateRequestAsync($"Aire {_token} uno", "Equipment", "High");
        var infrastructureHigh = await _client.CreateRequestAsync($"Luz {_token} dos", "Infrastructure", "High");
        var softwareLow = await _client.CreateRequestAsync($"Red {_token} tres", "Software", "Low");
        (await _client.PostAsUserAsync(
            $"{ApiClient.MaintenanceRequests}/{softwareLow}/status-changes", new { targetStatus = "Cancelled" }, TestUsers.Ana))
            .EnsureSuccessStatusCode();

        var byPriority = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}&priority=High");
        var byPriorityAndCategory = await _client.GetJsonAsync(
            $"{ApiClient.MaintenanceRequests}?search={_token}&priority=High&category=Infrastructure");
        var byStatus = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}&status=Cancelled");

        byPriority.GetProperty("totalCount").GetInt32().ShouldBe(2);
        Ids(byPriorityAndCategory).ShouldBe([infrastructureHigh]);
        Ids(byStatus).ShouldBe([softwareLow]);
    }

    [Fact]
    public async Task Search_TreatsWildcardsAsLiteralText()
    {
        var withPercent = await _client.CreateRequestAsync($"Descuento {_token} 50% aplicado", "Other", "Low");
        await _client.CreateRequestAsync($"Descuento {_token} 500 aplicado", "Other", "Low");
        var withUnderscore = await _client.CreateRequestAsync($"Codigo {_token}_x", "Other", "Low");
        await _client.CreateRequestAsync($"Codigo {_token}ax", "Other", "Low");

        var percent = await _client.GetJsonAsync(
            $"{ApiClient.MaintenanceRequests}?search={Uri.EscapeDataString($"{_token} 50%")}");
        var underscore = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}_");

        Ids(percent).ShouldBe([withPercent]);
        Ids(underscore).ShouldBe([withUnderscore]);
    }

    [Fact]
    public async Task Paging_ReturnsRequestedPageAndEmptyItemsOutOfRange()
    {
        for (var i = 0; i < 3; i++)
        {
            await _client.CreateRequestAsync($"Solicitud {_token} {i}", "Other", "Medium");
        }

        var firstPage = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}&pageSize=2&page=1");
        var secondPage = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}&pageSize=2&page=2");
        var outOfRange = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}&pageSize=2&page=5");

        firstPage.GetProperty("items").GetArrayLength().ShouldBe(2);
        firstPage.GetProperty("page").GetInt32().ShouldBe(1);
        firstPage.GetProperty("pageSize").GetInt32().ShouldBe(2);
        firstPage.GetProperty("totalCount").GetInt32().ShouldBe(3);
        firstPage.GetProperty("totalPages").GetInt32().ShouldBe(2);
        secondPage.GetProperty("items").GetArrayLength().ShouldBe(1);
        outOfRange.GetProperty("items").GetArrayLength().ShouldBe(0);
        outOfRange.GetProperty("totalCount").GetInt32().ShouldBe(3);
    }

    [Fact]
    public async Task ListItem_HasContractShape()
    {
        await _client.CreateRequestAsync($"Aire {_token}", "Equipment", "Critical");

        var page = await _client.GetJsonAsync($"{ApiClient.MaintenanceRequests}?search={_token}");

        var item = page.GetProperty("items")[0];
        item.GetProperty("title").GetString().ShouldBe($"Aire {_token}");
        item.GetProperty("category").GetString().ShouldBe("Equipment");
        item.GetProperty("priority").GetString().ShouldBe("Critical");
        item.GetProperty("status").GetString().ShouldBe("Pending");
        item.GetProperty("assignee").ValueKind.ShouldBe(JsonValueKind.Null);
        item.GetProperty("createdAt").GetString().ShouldEndWith("Z");
        item.TryGetProperty("description", out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("page=0", "page")]
    [InlineData("page=abc", "page")]
    [InlineData("pageSize=0", "pageSize")]
    [InlineData("pageSize=51", "pageSize")]
    [InlineData("status=1", "status")]
    [InlineData("status=Closed", "status")]
    [InlineData("priority=High,Low", "priority")]
    [InlineData("sortDirection=up", "sortDirection")]
    public async Task InvalidParameters_Return400WithFieldError(string queryString, string field)
    {
        var response = await _client.GetAsync($"{ApiClient.MaintenanceRequests}?{queryString}");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
        problem.GetProperty("errors").TryGetProperty(field, out _).ShouldBeTrue(problem.ToString());
    }

    [Fact]
    public async Task SearchLongerThan120Characters_Returns400()
    {
        var response = await _client.GetAsync($"{ApiClient.MaintenanceRequests}?search={new string('a', 121)}");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
        problem.GetProperty("errors").TryGetProperty("search", out _).ShouldBeTrue(problem.ToString());
    }

    [Fact]
    public async Task EnumNames_AreCaseInsensitive()
    {
        var response = await _client.GetAsync($"{ApiClient.MaintenanceRequests}?status=pending&sortDirection=ASC");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static List<Guid> Ids(JsonElement page) =>
        page.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("id").GetGuid()).ToList();
}
