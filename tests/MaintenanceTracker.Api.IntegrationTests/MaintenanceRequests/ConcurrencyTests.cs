using MaintenanceTracker.Api.IntegrationTests.Infrastructure;
using MaintenanceTracker.Application;
using MaintenanceTracker.Application.Errors;
using MaintenanceTracker.Application.MaintenanceRequests;
using MaintenanceTracker.Domain.MaintenanceRequests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace MaintenanceTracker.Api.IntegrationTests.MaintenanceRequests;

[Collection(ApiCollection.Name)]
public sealed class ConcurrencyTests(ApiFactory factory)
{
    // Deterministic on purpose: both contexts read before either one saves, then they save in sequence.
    // Parallel requests would depend on timing and could pass or fail at random.
    [Fact]
    public async Task SecondSaveOverStaleData_ThrowsConcurrencyConflict()
    {
        var created = await factory.CreateClient().CreateRequestAsync(TestUsers.Ana);

        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();

        var first = await LoadAsync(firstScope, created.Id);
        var second = await LoadAsync(secondScope, created.Id);

        first.AssignTo(TestUsers.Carlos, TestUsers.Ana, DateTimeOffset.UtcNow);
        await firstScope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);

        second.ChangeStatus(RequestStatus.Cancelled, TestUsers.Laura, DateTimeOffset.UtcNow);
        var exception = await Should.ThrowAsync<UseCaseException>(
            () => secondScope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None));

        exception.Code.ShouldBe(ApplicationErrorCodes.ConcurrencyConflict);

        var activity = await factory.GetStoredActivityAsync(created.Id);
        activity.Count.ShouldBe(2);
        activity.ShouldNotContain(entry => entry is StatusChangedActivity);
    }

    private static async Task<MaintenanceRequest> LoadAsync(AsyncServiceScope scope, Guid id) =>
        (await scope.ServiceProvider
            .GetRequiredService<IMaintenanceRequestRepository>()
            .GetByIdAsync(id, CancellationToken.None))!;
}
