using System.Net;
using MaintenanceTracker.Api.IntegrationTests.Infrastructure;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.MaintenanceRequests;

[Collection(ApiCollection.Name)]
public sealed class RequestValidationTests(ApiFactory factory)
{
    private const string ValidTitle = "\"title\":\"Aire acondicionado sin enfriar\"";
    private const string ValidDescription = "\"description\":\"El equipo de la sala 2 no responde.\"";

    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData($"{{{ValidDescription},\"category\":\"Equipment\",\"priority\":\"High\"}}", "title")]
    [InlineData($"{{{ValidTitle},{ValidDescription},\"priority\":\"High\"}}", "category")]
    [InlineData($"{{{ValidTitle},{ValidDescription},\"category\":\"Furniture\",\"priority\":\"High\"}}", "category")]
    [InlineData($"{{{ValidTitle},{ValidDescription},\"category\":1,\"priority\":\"High\"}}", "category")]
    [InlineData($"{{{ValidTitle},{ValidDescription},\"category\":null,\"priority\":\"High\"}}", "category")]
    [InlineData($"{{\"title\":\"  abc  \",{ValidDescription},\"category\":\"Equipment\",\"priority\":\"High\"}}", "title")]
    [InlineData($"{{{ValidTitle},\"description\":\"corta\",\"category\":\"Equipment\",\"priority\":\"High\"}}", "description")]
    public async Task InvalidCreateBody_Returns400WithFieldError(string json, string field)
    {
        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, json, TestUsers.Ana.ToString());

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
        problem.GetProperty("errors").TryGetProperty(field, out _).ShouldBeTrue(problem.ToString());
    }

    [Fact]
    public async Task CreateBodyWithUnexpectedField_Returns400()
    {
        var json = $"{{{ValidTitle},{ValidDescription},\"category\":\"Equipment\",\"priority\":\"High\",\"status\":\"Resolved\"}}";

        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, json, TestUsers.Ana.ToString());

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
    }

    [Fact]
    public async Task MalformedJson_Returns400()
    {
        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, "{\"title\":", TestUsers.Ana.ToString());

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
        // Only the parse error: no extra "body is required" error from the framework.
        problem.GetProperty("errors").TryGetProperty("body", out _).ShouldBeFalse(problem.ToString());
    }

    [Fact]
    public async Task EmptyBody_Returns400()
    {
        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, string.Empty, TestUsers.Ana.ToString());

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
        problem.GetProperty("errors").TryGetProperty("body", out _).ShouldBeTrue(problem.ToString());
    }

    [Fact]
    public async Task MissingUserHeader_Returns400UserRequired()
    {
        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, ValidJson(), userIdHeader: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "user_required");
    }

    [Fact]
    public async Task MalformedUserHeader_Returns400ValidationFailed()
    {
        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, ValidJson(), "not-a-uuid");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
        problem.GetProperty("errors").TryGetProperty("X-User-Id", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task UnknownUserHeader_Returns400UserNotFound()
    {
        var response = await _client.PostRawAsync(ApiClient.MaintenanceRequests, ValidJson(), TestUsers.Unknown.ToString());

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "user_not_found");
    }

    [Fact]
    public async Task ChangeStatusWithNumericTarget_Returns400()
    {
        var created = await _client.CreateRequestAsync(TestUsers.Ana);

        var response = await _client.PostRawAsync(
            $"{ApiClient.MaintenanceRequests}/{created.Id}/status-changes", "{\"targetStatus\":4}", TestUsers.Ana.ToString());

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed");
    }

    private static string ValidJson() =>
        $"{{{ValidTitle},{ValidDescription},\"category\":\"Equipment\",\"priority\":\"High\"}}";
}
