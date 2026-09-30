using System.Text.Json.Serialization;
using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

// The contract is a discriminated union on "type"; System.Text.Json writes the discriminator
// from these attributes (part of the BCL, so Application still has no NuGet packages).
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CreatedActivityDto), "Created")]
[JsonDerivedType(typeof(StatusChangedActivityDto), "StatusChanged")]
[JsonDerivedType(typeof(AssigneeChangedActivityDto), "AssigneeChanged")]
public abstract record ActivityEntryDto(Guid Id, DateTimeOffset OccurredAt, UserSummary PerformedBy);

public sealed record CreatedActivityDto(Guid Id, DateTimeOffset OccurredAt, UserSummary PerformedBy)
    : ActivityEntryDto(Id, OccurredAt, PerformedBy);

public sealed record StatusChangedActivityDto(
    Guid Id,
    DateTimeOffset OccurredAt,
    UserSummary PerformedBy,
    RequestStatus FromStatus,
    RequestStatus ToStatus)
    : ActivityEntryDto(Id, OccurredAt, PerformedBy);

public sealed record AssigneeChangedActivityDto(
    Guid Id,
    DateTimeOffset OccurredAt,
    UserSummary PerformedBy,
    UserSummary? PreviousAssignee,
    UserSummary NewAssignee)
    : ActivityEntryDto(Id, OccurredAt, PerformedBy);
