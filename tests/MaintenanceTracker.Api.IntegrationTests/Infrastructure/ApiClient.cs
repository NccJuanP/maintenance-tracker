using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MaintenanceTracker.Application.MaintenanceRequests;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.Infrastructure;

internal static class ApiClient
{
    public const string MaintenanceRequests = "/api/maintenance-requests";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static object ValidCreateBody() => new
    {
        title = "Aire acondicionado sin enfriar",
        description = "El equipo de la sala 2 marca 27 °C y no responde al control.",
        category = "Equipment",
        priority = "Critical"
    };

    public static Task<HttpResponseMessage> PostAsUserAsync(
        this HttpClient client, string url, object body, Guid userId) =>
        client.PostRawAsync(url, JsonSerializer.Serialize(body, Json), userId.ToString());

    public static Task<HttpResponseMessage> PostRawAsync(
        this HttpClient client, string url, string json, string? userIdHeader)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (userIdHeader is not null)
        {
            request.Headers.Add("X-User-Id", userIdHeader);
        }

        return client.SendAsync(request);
    }

    public static async Task<MaintenanceRequestDetail> CreateRequestAsync(this HttpClient client, Guid userId)
    {
        var response = await client.PostAsUserAsync(MaintenanceRequests, ValidCreateBody(), userId);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await response.ReadDetailAsync();
    }

    public static async Task<MaintenanceRequestDetail> ReadDetailAsync(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<MaintenanceRequestDetail>(Json))!;

    public static async Task<JsonElement> ShouldBeProblemAsync(
        this HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.ShouldBe(status);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("code").GetString().ShouldBe(code);
        problem.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
        return problem;
    }
}
