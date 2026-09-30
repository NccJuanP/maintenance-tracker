using System.Text.Json.Serialization;
using MaintenanceTracker.Api.Binding;
using MaintenanceTracker.Api.CurrentUser;
using MaintenanceTracker.Api.Errors;
using MaintenanceTracker.Api.Serialization;
using MaintenanceTracker.Application.MaintenanceRequests;
using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<GetUsers>();
builder.Services.AddScoped<GetMaintenanceRequestDetail>();
builder.Services.AddScoped<CreateMaintenanceRequest>();
builder.Services.AddScoped<AssignMaintenanceRequest>();
builder.Services.AddScoped<ChangeMaintenanceRequestStatus>();
builder.Services.AddScoped<ListMaintenanceRequests>();
builder.Services.AddScoped<GetMaintenanceRequestActivity>();
builder.Services.AddScoped<GetRequestSummary>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, HeaderCurrentUserProvider>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Location")));

builder.Services
    .AddControllers(options =>
    {
        // Keeps the Async suffix in action names so CreatedAtAction(nameof(GetByIdAsync)) resolves.
        options.SuppressAsyncSuffixInActionNames = false;
        options.ModelBinderProviders.Insert(0, new EnumNameModelBinderProvider());
        SpanishModelBindingMessages.Apply(options.ModelBindingMessageProvider);
        // Otherwise a body that fails to parse adds a second, English "body is required" error.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeOffsetConverter());
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        // The raw serializer messages mention internal type names.
        options.AllowInputFormatterExceptionMessages = false;
    })
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = InvalidModelStateResponse.Create);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await app.Services.ApplyMigrationsAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapControllers();

await app.RunAsync();

// Exposes the entry point to WebApplicationFactory in the integration tests.
public partial class Program;
