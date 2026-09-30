using MaintenanceTracker.Domain.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceTracker.Api.Errors;

internal sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = ToProblem(httpContext, exception);

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails ToProblem(HttpContext httpContext, Exception exception) => exception switch
    {
        DomainValidationException e => Problems.Validation(httpContext, Errors(e.Field, e.Message), e.Message),
        DomainException e => Problems.ForCode(httpContext, e.Code, e.Message),
        ApiRequestException { Field: not null } e => Problems.Validation(httpContext, Errors(e.Field, e.Message), e.Message),
        ApiRequestException e => Problems.ForCode(httpContext, e.Code, e.Message),
        _ => Problems.ForCode(
            httpContext,
            ApiErrorCodes.InternalError,
            "Ocurrió un error inesperado. Si el problema persiste, comparte el traceId con soporte.")
    };

    private static Dictionary<string, string[]> Errors(string field, string message) =>
        new() { [field] = [message] };
}
