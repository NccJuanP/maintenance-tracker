using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceTracker.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.Name).HasMaxLength(100);

        builder.HasData(
            new User { Id = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b01"), Name = "Ana Restrepo" },
            new User { Id = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b02"), Name = "Laura Gómez" },
            new User { Id = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b03"), Name = "Carlos Mejía" },
            new User { Id = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b04"), Name = "Andrés Ríos" },
            new User { Id = Guid.Parse("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b05"), Name = "Sofía Vélez" });
    }
}
