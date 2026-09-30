namespace MaintenanceTracker.Domain.Errors;

public sealed class DomainValidationException(string field, string message)
    : DomainException(DomainErrorCodes.ValidationFailed, message)
{
    public string Field { get; } = field;
}
