using MaintenanceTracker.Application.MaintenanceRequests;
using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;
using MaintenanceTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceTracker.Infrastructure.MaintenanceRequests;

internal sealed class MaintenanceRequestQueries(MaintenanceTrackerDbContext dbContext) : IMaintenanceRequestQueries
{
    public async Task<MaintenanceRequestDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await (
                from request in dbContext.MaintenanceRequests.AsNoTracking()
                join requester in dbContext.Users on request.RequesterId equals requester.Id
                join assignee in dbContext.Users on request.AssigneeId equals assignee.Id into assignees
                from assignee in assignees.DefaultIfEmpty()
                where request.Id == id
                select new
                {
                    request.Id,
                    request.Title,
                    request.Description,
                    request.Category,
                    request.Priority,
                    request.Status,
                    Assignee = assignee == null ? null : new UserSummary(assignee.Id, assignee.Name),
                    Requester = new UserSummary(requester.Id, requester.Name),
                    request.CreatedAt
                })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        // Derived from the same rules the aggregate enforces, without loading the aggregate.
        return new MaintenanceRequestDetail(
            row.Id,
            row.Title,
            row.Description,
            row.Category,
            row.Priority,
            row.Status,
            row.Assignee,
            row.Requester,
            row.CreatedAt,
            StatusTransitions.AllowedFrom(row.Status, hasAssignee: row.Assignee is not null),
            CanChangeAssignee: !StatusTransitions.IsFinal(row.Status));
    }
}
