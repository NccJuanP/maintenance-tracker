# CLAUDE.md

Guía para Claude Code en este repositorio. Léela completa antes de planificar o escribir código.

## Contexto

Prueba técnica Full Stack Mid-Level para Expertos Seguridad LTDA: aplicación de gestión de solicitudes de mantenimiento. Se evalúa sobre todo la arquitectura, las reglas de negocio y la capacidad de justificar decisiones, no la cantidad de funcionalidades ni de librerías. El autor debe poder explicar cada línea en una sustentación, así que se prefiere código simple y explícito sobre código ingenioso.

## Documentos de referencia (fuente de verdad)

1. `docs/domain/event-storming.md`: lenguaje ubicuo, reglas, decisiones D1-D12, endpoints, tipos y códigos de error.
2. `docs/architecture/tactical-design.md`: estructura, patrones, persistencia y pruebas (decisiones T1-T34).

Si una tarea contradice estos documentos, detente y pregunta antes de implementar. Si una decisión cambia, actualiza el documento correspondiente en el mismo cambio. No inventes endpoints, campos ni códigos de error que no estén en el contrato.

## Estructura

```
/
├── MaintenanceTracker.sln     ← formato .sln, no .slnx (T22)
├── Directory.Build.props     ← TargetFramework, Nullable, ImplicitUsings (T23)
├── .config/dotnet-tools.json ← dotnet-ef 8.x (T29)
├── docker-compose.yml
├── .env.example
├── docs/
├── src/
│   ├── MaintenanceTracker.Domain
│   ├── MaintenanceTracker.Application
│   ├── MaintenanceTracker.Infrastructure
│   └── MaintenanceTracker.Api
├── tests/
│   ├── MaintenanceTracker.Domain.Tests
│   └── MaintenanceTracker.Api.IntegrationTests
└── web/                      ← frontend Next.js (se trabaja por separado)
```

Reglas de dependencia (no negociables):

- `Domain` no referencia ningún proyecto ni paquete NuGet.
- `Application` solo referencia `Domain`, sin paquetes NuGet.
- Los controladores solo usan tipos de `Application`. `Infrastructure` se referencia desde `Api` únicamente para registrar dependencias.
- Nunca se exponen entidades de dominio ni de persistencia en la API.

## Librerías

Permitidas: las de la sección 8 de `tactical-design.md`, en versiones 8.x donde aplique (el proyecto es .NET 8).

Prohibidas: MediatR, AutoMapper, FluentValidation, FluentAssertions. No agregues ninguna otra sin preguntar y justificar.

## Convenciones de código

- Código, nombres y mensajes de log en inglés. Mensajes de error de la API en español (los ve el usuario final); los `code` en inglés `snake_case`.
- `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>` en todos los proyectos, definidos una sola vez en `Directory.Build.props` (no repetirlos en cada `.csproj`). Namespaces con ámbito de archivo.
- Clases `sealed` por defecto. `record` para modelos de respuesta, cuerpos de petición y comandos.
- Métodos asíncronos con sufijo `Async` y `CancellationToken` propagado hasta EF Core.
- Entidades de dominio sin setters públicos; se modifican solo por sus métodos.
- Fechas siempre en UTC (`DateTimeOffset`), obtenidas de `TimeProvider` en la capa de aplicación.
- Sin comentarios obvios ni comentarios que citen IDs de decisiones (`D6`, `T4`, etc.): las decisiones evolucionan y esas referencias quedan desactualizadas. La trazabilidad vive en `docs/`.

## Infraestructura y ejecución

- Cada proyecto nuevo que forme parte del build de la API debe agregar su `COPY` del `.csproj` en `src/MaintenanceTracker.Api/Dockerfile` antes del `dotnet restore`. `Directory.Build.props` también se copia antes del `restore`.
- La solución es `.sln`: si hace falta recrearla, usa `dotnet new sln --format sln`, porque los SDK 9+ generan `.slnx` por defecto y el evaluador puede tener solo el SDK 8.
- Toda configuración nueva entra por variables de entorno: se agrega a `docker-compose.yml` y a `.env.example`, sin secretos reales.
- La solución debe seguir levantando con `docker compose up -d --build` después de cada fase.

Comandos útiles:

```bash
dotnet build
dotnet test tests/MaintenanceTracker.Domain.Tests
dotnet test tests/MaintenanceTracker.Api.IntegrationTests   # requiere Docker corriendo
dotnet tool restore                                         # solo para crear migraciones (T29)
# la API exige una cadena de conexión al arrancar; para crear migraciones basta una ficticia
ConnectionStrings__Default="Host=localhost" dotnet ef migrations add <Nombre> -p src/MaintenanceTracker.Infrastructure -s src/MaintenanceTracker.Api -o Persistence/Migrations
docker compose up -d --build
```

## Forma de trabajo

- Trabaja por fases pequeñas. Al cerrar cada una: compila, pasan las pruebas existentes y la solución levanta con Docker Compose.
- En cada fase, empieza por el dominio y sus pruebas antes de la infraestructura.
- Al terminar una fase, resume qué hiciste, qué decisiones tomaste que no estuvieran en los documentos y cómo verificarlo manualmente.
- Propón un mensaje de commit en formato Conventional Commits, en español (por ejemplo `feat: agregar máquina de estados del agregado`).
- No modifiques `web/` salvo que se pida explícitamente.