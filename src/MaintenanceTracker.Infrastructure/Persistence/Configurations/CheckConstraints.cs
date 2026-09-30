namespace MaintenanceTracker.Infrastructure.Persistence.Configurations;

internal static class CheckConstraints
{
    public const int EnumMaxLength = 20;

    public static string EnumValues<TEnum>(string column) where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"))})";
}
