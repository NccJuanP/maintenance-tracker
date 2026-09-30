using System.ComponentModel.DataAnnotations;
using MaintenanceTracker.Application.MaintenanceRequests;
using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Api.Contracts;

public record PagingParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser un entero mayor o igual a 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 50, ErrorMessage = "El tamaño de página debe estar entre 1 y 50.")]
    public int PageSize { get; init; } = 10;
}

public sealed record MaintenanceRequestListParameters : PagingParameters
{
    public RequestStatus? Status { get; init; }
    public Priority? Priority { get; init; }
    public Category? Category { get; init; }

    [StringLength(120, ErrorMessage = "La búsqueda admite hasta 120 caracteres.")]
    public string? Search { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
