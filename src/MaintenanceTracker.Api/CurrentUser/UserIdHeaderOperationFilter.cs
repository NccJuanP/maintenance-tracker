using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MaintenanceTracker.Api.CurrentUser;

// The header is read by HeaderCurrentUserProvider, not bound as an action parameter,
// so Swagger would not know about it and could not send it from its UI.
internal sealed class UserIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!HttpMethods.IsPost(context.ApiDescription.HttpMethod ?? string.Empty))
        {
            return;
        }

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderCurrentUserProvider.HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Usuario que realiza la operación. Los disponibles están en GET /api/users.",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        });
    }
}
