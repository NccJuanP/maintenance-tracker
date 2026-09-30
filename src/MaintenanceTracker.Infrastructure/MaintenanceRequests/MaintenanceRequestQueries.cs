using MaintenanceTracker.Application;
using MaintenanceTracker.Application.MaintenanceRequests;
using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;
using MaintenanceTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceTracker.Infrastructure.MaintenanceRequests;

internal sealed class MaintenanceRequestQueries(MaintenanceTrackerDbContext dbContext) : IMaintenanceRequestQueries
{
    private const string LikeEscape = "\\";

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

    public async Task<PagedResult<MaintenanceRequestListItem>> ListAsync(
        MaintenanceRequestListQuery query, CancellationToken cancellationToken)
    {
        var filtered = dbContext.MaintenanceRequests.AsNoTracking();

        if (query.Status is { } status)
        {
            filtered = filtered.Where(request => request.Status == status);
        }

        if (query.Priority is { } priority)
        {
            filtered = filtered.Where(request => request.Priority == priority);
        }

        if (query.Category is { } category)
        {
            filtered = filtered.Where(request => request.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLikePattern(query.Search.Trim())}%";
            filtered = filtered.Where(request => EF.Functions.ILike(request.Title, pattern, LikeEscape));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var joined =
            from request in filtered
            join assignee in dbContext.Users on request.AssigneeId equals assignee.Id into assignees
            from assignee in assignees.DefaultIfEmpty()
            select new { Request = request, Assignee = assignee };

        // Ties on created_at are broken by id so pages never overlap or skip rows.
        joined = query.SortDirection == SortDirection.Asc
            ? joined.OrderBy(row => row.Request.CreatedAt).ThenBy(row => row.Request.Id)
            : joined.OrderByDescending(row => row.Request.CreatedAt).ThenByDescending(row => row.Request.Id);

        var items = await joined
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(row => new MaintenanceRequestListItem(
                row.Request.Id,
                row.Request.Title,
                row.Request.Category,
                row.Request.Priority,
                row.Request.Status,
                row.Assignee == null ? null : new UserSummary(row.Assignee.Id, row.Assignee.Name),
                row.Request.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<MaintenanceRequestListItem>.Create(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PagedResult<ActivityEntryDto>?> GetActivityAsync(
        Guid maintenanceRequestId, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (!await dbContext.MaintenanceRequests.AnyAsync(request => request.Id == maintenanceRequestId, cancellationToken))
        {
            return null;
        }

        var entries = dbContext.ActivityEntries
            .AsNoTracking()
            .Where(entry => entry.MaintenanceRequestId == maintenanceRequestId);

        var totalCount = await entries.CountAsync(cancellationToken);

        var pageEntries = await entries
            .OrderBy(entry => entry.OccurredAt)
            .ThenBy(entry => entry.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Entries reference up to three users; resolving them in one lookup keeps the page query simple.
        var userIds = pageEntries.SelectMany(ReferencedUserIds).Distinct().ToList();
        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => new UserSummary(user.Id, user.Name), cancellationToken);

        var items = pageEntries.Select(entry => ToDto(entry, users)).ToList();

        return PagedResult<ActivityEntryDto>.Create(items, page, pageSize, totalCount);
    }

    public async Task<RequestSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var counts = await dbContext.MaintenanceRequests
            .GroupBy(request => request.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Status, row => row.Count, cancellationToken);

        // Statuses without requests are not in the GROUP BY result, but the contract always lists all five.
        var byStatus = Enum.GetValues<RequestStatus>()
            .ToDictionary(status => status, status => counts.GetValueOrDefault(status));

        return new RequestSummary(byStatus.Values.Sum(), byStatus);
    }

    private static string EscapeLikePattern(string text) =>
        text.Replace(LikeEscape, LikeEscape + LikeEscape)
            .Replace("%", LikeEscape + "%")
            .Replace("_", LikeEscape + "_");

    private static IEnumerable<Guid> ReferencedUserIds(ActivityEntry entry)
    {
        yield return entry.PerformedById;

        if (entry is AssigneeChangedActivity assigneeChanged)
        {
            if (assigneeChanged.PreviousAssigneeId is { } previousAssigneeId)
            {
                yield return previousAssigneeId;
            }

            yield return assigneeChanged.NewAssigneeId;
        }
    }

    private static ActivityEntryDto ToDto(ActivityEntry entry, IReadOnlyDictionary<Guid, UserSummary> users) =>
        entry switch
        {
            CreatedActivity created => new CreatedActivityDto(
                created.Id, created.OccurredAt, users[created.PerformedById]),
            StatusChangedActivity statusChanged => new StatusChangedActivityDto(
                statusChanged.Id,
                statusChanged.OccurredAt,
                users[statusChanged.PerformedById],
                statusChanged.FromStatus,
                statusChanged.ToStatus),
            AssigneeChangedActivity assigneeChanged => new AssigneeChangedActivityDto(
                assigneeChanged.Id,
                assigneeChanged.OccurredAt,
                users[assigneeChanged.PerformedById],
                assigneeChanged.PreviousAssigneeId is { } previousId ? users[previousId] : null,
                users[assigneeChanged.NewAssigneeId]),
            _ => throw new InvalidOperationException($"Unknown activity entry type {entry.GetType().Name}.")
        };
}
