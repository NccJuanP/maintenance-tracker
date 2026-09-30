using MaintenanceTracker.Domain.Errors;

namespace MaintenanceTracker.Domain.MaintenanceRequests;

public sealed class MaintenanceRequest
{
    public const int TitleMinLength = 5;
    public const int TitleMaxLength = 120;
    public const int DescriptionMinLength = 10;
    public const int DescriptionMaxLength = 2000;

    private readonly List<ActivityEntry> _activityEntries = [];

    private MaintenanceRequest(
        Guid id,
        string title,
        string description,
        Category category,
        Priority priority,
        RequestStatus status,
        Guid requesterId,
        Guid? assigneeId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        Priority = priority;
        Status = status;
        RequesterId = requesterId;
        AssigneeId = assigneeId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Category Category { get; private set; }
    public Priority Priority { get; private set; }
    public RequestStatus Status { get; private set; }
    public Guid RequesterId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<ActivityEntry> ActivityEntries => _activityEntries.AsReadOnly();

    public static MaintenanceRequest Create(
        string title,
        string description,
        Category category,
        Priority priority,
        Guid requesterId,
        DateTimeOffset occurredAt)
    {
        var trimmedTitle = title.Trim();
        var trimmedDescription = description.Trim();

        if (trimmedTitle.Length is < TitleMinLength or > TitleMaxLength)
        {
            throw new DomainValidationException(
                "title",
                $"El título debe tener entre {TitleMinLength} y {TitleMaxLength} caracteres.");
        }

        if (trimmedDescription.Length is < DescriptionMinLength or > DescriptionMaxLength)
        {
            throw new DomainValidationException(
                "description",
                $"La descripción debe tener entre {DescriptionMinLength} y {DescriptionMaxLength} caracteres.");
        }

        if (!Enum.IsDefined(category))
        {
            throw new DomainValidationException("category", "La categoría no es válida.");
        }

        if (!Enum.IsDefined(priority))
        {
            throw new DomainValidationException("priority", "La prioridad no es válida.");
        }

        var request = new MaintenanceRequest(
            Guid.NewGuid(),
            trimmedTitle,
            trimmedDescription,
            category,
            priority,
            RequestStatus.Pending,
            requesterId,
            assigneeId: null,
            createdAt: occurredAt);

        request._activityEntries.Add(new CreatedActivity(Guid.NewGuid(), request.Id, occurredAt, requesterId));

        return request;
    }

    public void AssignTo(Guid assigneeId, Guid performedById, DateTimeOffset occurredAt)
    {
        if (StatusTransitions.IsFinal(Status))
        {
            throw new DomainException(
                DomainErrorCodes.RequestClosed,
                "No se puede cambiar el responsable de una solicitud resuelta o cancelada.");
        }

        if (AssigneeId == assigneeId)
        {
            throw new DomainException(
                DomainErrorCodes.AssigneeUnchanged,
                "El usuario indicado ya es el responsable de la solicitud.");
        }

        var previousAssigneeId = AssigneeId;
        AssigneeId = assigneeId;

        _activityEntries.Add(new AssigneeChangedActivity(
            Guid.NewGuid(), Id, occurredAt, performedById, previousAssigneeId, assigneeId));
    }

    public void ChangeStatus(RequestStatus targetStatus, Guid performedById, DateTimeOffset occurredAt)
    {
        if (!Enum.IsDefined(targetStatus))
        {
            throw new DomainValidationException("targetStatus", "El estado destino no es válido.");
        }

        if (!StatusTransitions.IsAllowed(Status, targetStatus))
        {
            throw new DomainException(
                DomainErrorCodes.InvalidStatusTransition,
                $"No se permite pasar la solicitud de {Status} a {targetStatus}.");
        }

        if (StatusTransitions.RequiresAssignee(targetStatus) && AssigneeId is null)
        {
            throw new DomainException(
                DomainErrorCodes.AssigneeRequired,
                "La solicitud necesita un responsable para pasar a en progreso.");
        }

        var fromStatus = Status;
        Status = targetStatus;

        _activityEntries.Add(new StatusChangedActivity(
            Guid.NewGuid(), Id, occurredAt, performedById, fromStatus, targetStatus));
    }
}
