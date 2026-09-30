using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace MaintenanceTracker.Api.Errors;

// The framework's binding messages are in English and reach the end user through validation_failed.
internal static class SpanishModelBindingMessages
{
    public static void Apply(DefaultModelBindingMessageProvider messages)
    {
        messages.SetMissingBindRequiredValueAccessor(field => $"Falta el valor de '{field}'.");
        messages.SetMissingKeyOrValueAccessor(() => "Falta un valor obligatorio.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "El cuerpo de la petición es obligatorio.");
        messages.SetValueMustNotBeNullAccessor(value => "El valor no puede ser nulo.");
        messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"El valor '{value}' no es válido para '{field}'.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetUnknownValueIsInvalidAccessor(field => $"El valor de '{field}' no es válido.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor no es válido.");
        messages.SetValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetValueMustBeANumberAccessor(field => $"'{field}' debe ser un número.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número.");
    }
}
