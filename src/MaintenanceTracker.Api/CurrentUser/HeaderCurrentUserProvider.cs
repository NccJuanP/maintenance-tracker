using MaintenanceTracker.Api.Errors;
using MaintenanceTracker.Domain.Errors;

namespace MaintenanceTracker.Api.CurrentUser;

internal sealed class HeaderCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public const string HeaderName = "X-User-Id";

    public Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiRequestException(
                ApiErrorCodes.UserRequired,
                $"Falta el encabezado {HeaderName} con el usuario que realiza la operación.");
        }

        if (!Guid.TryParse(value, out var userId))
        {
            throw new ApiRequestException(
                DomainErrorCodes.ValidationFailed,
                $"El encabezado {HeaderName} debe ser un UUID.",
                HeaderName);
        }

        return userId;
    }
}
