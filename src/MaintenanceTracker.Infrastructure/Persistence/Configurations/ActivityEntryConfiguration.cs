using MaintenanceTracker.Domain.MaintenanceRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceTracker.Infrastructure.Persistence.Configurations;

internal sealed class ActivityEntryConfiguration : IEntityTypeConfiguration<ActivityEntry>
{
    public const string Created = "Created";
    public const string StatusChanged = "StatusChanged";
    public const string AssigneeChanged = "AssigneeChanged";

    public void Configure(EntityTypeBuilder<ActivityEntry> builder)
    {
        builder.ToTable("activity_entries", table =>
        {
            // Subtype columns are nullable in a single table, so each type must declare exactly its own.
            table.HasCheckConstraint(
                "ck_activity_entries_type_columns",
                $"""
                (type = '{Created}' AND from_status IS NULL AND to_status IS NULL
                    AND previous_assignee_id IS NULL AND new_assignee_id IS NULL)
                OR (type = '{StatusChanged}' AND from_status IS NOT NULL AND to_status IS NOT NULL
                    AND previous_assignee_id IS NULL AND new_assignee_id IS NULL)
                OR (type = '{AssigneeChanged}' AND from_status IS NULL AND to_status IS NULL
                    AND new_assignee_id IS NOT NULL)
                """);
            table.HasCheckConstraint("ck_activity_entries_from_status", CheckConstraints.EnumValues<RequestStatus>("from_status"));
            table.HasCheckConstraint("ck_activity_entries_to_status", CheckConstraints.EnumValues<RequestStatus>("to_status"));
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.HasDiscriminator<string>("type")
            .HasValue<CreatedActivity>(Created)
            .HasValue<StatusChangedActivity>(StatusChanged)
            .HasValue<AssigneeChangedActivity>(AssigneeChanged);
        builder.Property<string>("type").HasMaxLength(CheckConstraints.EnumMaxLength);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.MaintenanceRequestId, e.OccurredAt, e.Id });
    }
}

internal sealed class StatusChangedActivityConfiguration : IEntityTypeConfiguration<StatusChangedActivity>
{
    public void Configure(EntityTypeBuilder<StatusChangedActivity> builder)
    {
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(CheckConstraints.EnumMaxLength);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(CheckConstraints.EnumMaxLength);
    }
}

internal sealed class AssigneeChangedActivityConfiguration : IEntityTypeConfiguration<AssigneeChangedActivity>
{
    public void Configure(EntityTypeBuilder<AssigneeChangedActivity> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.PreviousAssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.NewAssigneeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
