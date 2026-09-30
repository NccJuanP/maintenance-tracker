namespace MaintenanceTracker.Api.CurrentUser;

// The only place that knows where the current user comes from; real authentication would replace it.
public interface ICurrentUserProvider
{
    Guid GetUserId();
}
