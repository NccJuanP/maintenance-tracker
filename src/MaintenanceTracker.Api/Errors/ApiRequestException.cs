namespace MaintenanceTracker.Api.Errors;

// Errors detected by the API itself before reaching a use case, such as a missing or malformed header.
public sealed class ApiRequestException(string code, string message, string? field = null) : Exception(message)
{
    public string Code { get; } = code;
    public string? Field { get; } = field;
}
