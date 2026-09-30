using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MaintenanceTracker.Api.Binding;

// MVC's default enum binding also accepts numbers ("?status=1") and flag combinations ("Pending,InProgress").
// The contract uses enum names only, so anything else is a validation error.
internal sealed class EnumNameModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var enumType = bindingContext.ModelMetadata.UnderlyingOrModelType;
        var name = Enum.GetNames(enumType)
            .FirstOrDefault(candidate => string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase));

        if (name is null)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"El valor '{value}' no es válido.");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(Enum.Parse(enumType, name));
        return Task.CompletedTask;
    }
}

internal sealed class EnumNameModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.UnderlyingOrModelType.IsEnum ? new EnumNameModelBinder() : null;
}
