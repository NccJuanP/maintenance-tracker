using System.Diagnostics;
using MaintenanceTracker.Application.Errors;
using MaintenanceTracker.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceTracker.Api.Errors;

internal static class Problems
{
    private static readonly Dictionary<string, int> StatusByCode = new()
    {
        [DomainErrorCodes.ValidationFailed] = StatusCodes.Status400BadRequest,
        [ApiErrorCodes.UserRequired] = StatusCodes.Status400BadRequest,
        [ApplicationErrorCodes.UserNotFound] = StatusCodes.Status400BadRequest,
        [ApplicationErrorCodes.AssigneeNotFound] = StatusCodes.Status400BadRequest,
        [ApplicationErrorCodes.MaintenanceRequestNotFound] = StatusCodes.Status404NotFound,
        [DomainErrorCodes.InvalidStatusTransition] = StatusCodes.Status409Conflict,
        [DomainErrorCodes.AssigneeRequired] = StatusCodes.Status409Conflict,
        [DomainErrorCodes.RequestClosed] = StatusCodes.Status409Conflict,
        [DomainErrorCodes.AssigneeUnchanged] = StatusCodes.Status409Conflict,
        [ApplicationErrorCodes.ConcurrencyConflict] = StatusCodes.Status409Conflict,
        [ApiErrorCodes.InternalError] = StatusCodes.Status500InternalServerError
    };

    public static ProblemDetails ForCode(HttpContext httpContext, string code, string detail)
    {
        var status = StatusByCode.GetValueOrDefault(code, StatusCodes.Status500InternalServerError);
        return Complete(new ProblemDetails(), httpContext, status, code, detail);
    }

    public static ValidationProblemDetails Validation(
        HttpContext httpContext, IDictionary<string, string[]> errors, string detail) =>
        Complete(
            new ValidationProblemDetails(errors),
            httpContext,
            StatusCodes.Status400BadRequest,
            DomainErrorCodes.ValidationFailed,
            detail);

    private static TProblem Complete<TProblem>(
        TProblem problem, HttpContext httpContext, int status, string code, string detail)
        where TProblem : ProblemDetails
    {
        problem.Status = status;
        problem.Title = TitleFor(status);
        problem.Detail = detail;
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        return problem;
    }

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "La petición no es válida.",
        StatusCodes.Status404NotFound => "El recurso no existe.",
        StatusCodes.Status409Conflict => "La operación no es compatible con el estado actual.",
        _ => "Error interno del servidor."
    };
}
