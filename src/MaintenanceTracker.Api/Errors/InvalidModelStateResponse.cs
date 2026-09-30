using Microsoft.AspNetCore.Mvc;

namespace MaintenanceTracker.Api.Errors;

// Binding and JSON errors become validation_failed with the same shape as the domain validation errors.
internal static class InvalidModelStateResponse
{
    private const string GenericMessage = "El valor no es válido.";

    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .GroupBy(entry => FieldName(entry.Key))
            .ToDictionary(
                group => group.Key,
                group => group
                    .SelectMany(entry => entry.Value!.Errors)
                    .Select(error => string.IsNullOrEmpty(error.ErrorMessage) ? GenericMessage : error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var problem = Problems.Validation(context.HttpContext, errors, "Revisa los datos enviados.");

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }

    // JSON errors come keyed by path ("$.title"); the contract uses the plain field name.
    private static string FieldName(string key) => key switch
    {
        "" or "$" => "body",
        _ when key.StartsWith("$.", StringComparison.Ordinal) => key[2..],
        _ => key
    };
}
