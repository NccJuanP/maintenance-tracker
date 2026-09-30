using MaintenanceTracker.Domain.MaintenanceRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceTracker.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.ToTable("maintenance_requests", table =>
        {
            table.HasCheckConstraint(
                "ck_maintenance_requests_title_length",
                $"char_length(btrim(title)) BETWEEN {MaintenanceRequest.TitleMinLength} AND {MaintenanceRequest.TitleMaxLength}");
            table.HasCheckConstraint(
                "ck_maintenance_requests_description_length",
                $"char_length(btrim(description)) BETWEEN {MaintenanceRequest.DescriptionMinLength} AND {MaintenanceRequest.DescriptionMaxLength}");
            table.HasCheckConstraint("ck_maintenance_requests_category", CheckConstraints.EnumValues<Category>("category"));
            table.HasCheckConstraint("ck_maintenance_requests_priority", CheckConstraints.EnumValues<Priority>("priority"));
            table.HasCheckConstraint("ck_maintenance_requests_status", CheckConstraints.EnumValues<RequestStatus>("status"));
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Title).HasMaxLength(MaintenanceRequest.TitleMaxLength);
        builder.Property(r => r.Description).HasMaxLength(MaintenanceRequest.DescriptionMaxLength);
        builder.Property(r => r.Category).HasConversion<string>().HasMaxLength(CheckConstraints.EnumMaxLength);
        builder.Property(r => r.Priority).HasConversion<string>().HasMaxLength(CheckConstraints.EnumMaxLength);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(CheckConstraints.EnumMaxLength);

        // PostgreSQL's xmin system column changes on every update, so it works as a
        // concurrency token without the domain knowing about it.
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.ActivityEntries)
            .WithOne()
            .HasForeignKey(e => e.MaintenanceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(r => r.ActivityEntries).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => new { r.CreatedAt, r.Id });
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => r.Category);
        builder.HasIndex(r => r.Title)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
