using MaintenanceTracker.Domain.MaintenanceRequests;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceTracker.Infrastructure.Persistence;

public sealed class MaintenanceTrackerDbContext(DbContextOptions<MaintenanceTrackerDbContext> options)
    : DbContext(options)
{
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaintenanceTrackerDbContext).Assembly);
    }
}
