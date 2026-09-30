namespace MaintenanceTracker.Infrastructure.Persistence;

// Persistence-only: the domain references users by id and has no rules about them.
public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
