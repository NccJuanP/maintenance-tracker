# Etapa 1: compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/MaintenanceTracker.Domain/MaintenanceTracker.Domain.csproj src/MaintenanceTracker.Domain/
COPY src/MaintenanceTracker.Application/MaintenanceTracker.Application.csproj src/MaintenanceTracker.Application/
COPY src/MaintenanceTracker.Infrastructure/MaintenanceTracker.Infrastructure.csproj src/MaintenanceTracker.Infrastructure/
COPY src/MaintenanceTracker.Api/MaintenanceTracker.Api.csproj src/MaintenanceTracker.Api/
RUN dotnet restore src/MaintenanceTracker.Api/MaintenanceTracker.Api.csproj

COPY src/ src/
RUN dotnet publish src/MaintenanceTracker.Api/MaintenanceTracker.Api.csproj -c Release -o /app/publish --no-restore

# Etapa 2: ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "MaintenanceTracker.Api.dll"]