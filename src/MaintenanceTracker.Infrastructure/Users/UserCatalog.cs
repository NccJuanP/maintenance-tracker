using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceTracker.Infrastructure.Users;

internal sealed class UserCatalog(MaintenanceTrackerDbContext dbContext) : IUserCatalog
{
    public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .Select(user => new UserSummary(user.Id, user.Name))
            .ToListAsync(cancellationToken);
}
