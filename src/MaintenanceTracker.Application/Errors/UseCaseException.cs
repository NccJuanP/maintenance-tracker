namespace MaintenanceTracker.Application.Errors;

// Rules that need more than the aggregate to be checked: existence in the catalog or in storage, and concurrency.
public sealed class UseCaseException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;

    public static UseCaseException UserNotFound(Guid userId) =>
        new(ApplicationErrorCodes.UserNotFound, $"El usuario {userId} no existe.");

    public static UseCaseException AssigneeNotFound(Guid assigneeId) =>
        new(ApplicationErrorCodes.AssigneeNotFound, $"El responsable {assigneeId} no existe.");

    public static UseCaseException MaintenanceRequestNotFound(Guid maintenanceRequestId) =>
        new(ApplicationErrorCodes.MaintenanceRequestNotFound, $"La solicitud {maintenanceRequestId} no existe.");

    public static UseCaseException ConcurrencyConflict() =>
        new(ApplicationErrorCodes.ConcurrencyConflict,
            "Otro cambio se guardó primero sobre esta solicitud. Recarga la información e inténtalo de nuevo.");
}
