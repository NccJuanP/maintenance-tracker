namespace MaintenanceTracker.Application.Users;

public sealed class GetUsers(IUserCatalog userCatalog)
{
    public Task<IReadOnlyList<UserSummary>> ExecuteAsync(CancellationToken cancellationToken) =>
        userCatalog.ListAsync(cancellationToken);
}
