using System.ComponentModel.DataAnnotations;
using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Api.Contracts;

// Nullable on purpose: a missing field must fail with validation_failed instead of silently
// taking its default value (a missing enum would become its first member).

public sealed record CreateMaintenanceRequestBody(
    [Required(ErrorMessage = "El título es obligatorio.")] string? Title,
    [Required(ErrorMessage = "La descripción es obligatoria.")] string? Description,
    [Required(ErrorMessage = "La categoría es obligatoria.")] Category? Category,
    [Required(ErrorMessage = "La prioridad es obligatoria.")] Priority? Priority);

public sealed record AssignBody(
    [Required(ErrorMessage = "El responsable es obligatorio.")] Guid? AssigneeId);

public sealed record ChangeStatusBody(
    [Required(ErrorMessage = "El estado destino es obligatorio.")] RequestStatus? TargetStatus);
