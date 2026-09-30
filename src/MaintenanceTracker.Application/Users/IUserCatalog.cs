namespace MaintenanceTracker.Application.Users;

public interface IUserCatalog
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken);
}
