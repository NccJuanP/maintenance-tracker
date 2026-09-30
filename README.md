# MaintenanceTracker

API para gestionar solicitudes de mantenimiento: registrar una solicitud, asignarle un responsable, llevarla por su ciclo de vida (`Pending → InProgress ⇄ OnHold → Resolved`, o `Cancelled` desde cualquier estado no final) y consultar su historial, que registra cada cambio en la misma transacción que lo produce.

Backend en .NET 8 con PostgreSQL 17, separado en capas (dominio, aplicación, infraestructura y API).

## Ejecutar

Requisito: Docker con Docker Compose.

```bash
cp .env.example .env
docker compose up -d --build
```

- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger

Al arrancar, la API aplica las migraciones y siembra los usuarios de prueba. No hace falta ningún paso manual.

### Usuarios de prueba

No hay autenticación. Las escrituras (`POST`) identifican al usuario con el encabezado `X-User-Id`, que debe ser uno de estos:

| Id | Nombre |
|---|---|
| `0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b01` | Ana Restrepo |
| `0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b02` | Laura Gómez |
| `0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b03` | Carlos Mejía |
| `0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b04` | Andrés Ríos |
| `0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b05` | Sofía Vélez |

### Ejemplo

```bash
curl -X POST http://localhost:8080/api/maintenance-requests \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b01" \
  -d '{"title":"Fuga en el piso 3","description":"El lavamanos gotea desde ayer.","category":"Infrastructure","priority":"Medium"}'
```

En Windows, curl no envía en UTF-8 los caracteres no ASCII que van como argumento (tildes, ñ). Si el cuerpo los tiene, guárdalo en un archivo y envíalo con `--data-binary @archivo.json`, o usa Swagger.

## Endpoints

| Método y ruta | Descripción |
|---|---|
| `GET /api/users` | Catálogo de usuarios |
| `POST /api/maintenance-requests` | Crear solicitud (201 con `Location`) |
| `GET /api/maintenance-requests` | Listado paginado con filtros (`status`, `priority`, `category`, `search`, `sortDirection`, `page`, `pageSize`) |
| `GET /api/maintenance-requests/{id}` | Detalle, con `allowedTransitions` y `canChangeAssignee` |
| `POST /api/maintenance-requests/{id}/assignments` | Asignar o reasignar responsable |
| `POST /api/maintenance-requests/{id}/status-changes` | Cambiar estado |
| `GET /api/maintenance-requests/{id}/activity` | Historial paginado |
| `GET /api/maintenance-requests/summary` | Conteo por estado |

Los errores siguen ProblemDetails (RFC 9457) e incluyen un `code` estable (`validation_failed`, `invalid_status_transition`, `concurrency_conflict`, etc.) y un `traceId`. El contrato completo, con tipos y ejemplos, está en [`docs/domain/event-storming.md`](docs/domain/event-storming.md) (sección 5).

## Pruebas

Requieren el SDK de .NET 8. Las de integración además necesitan Docker corriendo, porque levantan un PostgreSQL real con Testcontainers.

```bash
dotnet test tests/MaintenanceTracker.Domain.Tests            # reglas de negocio, sin base de datos
dotnet test tests/MaintenanceTracker.Api.IntegrationTests    # API completa contra PostgreSQL
```

## Migraciones

Para ejecutar la aplicación o las pruebas no hace falta instalar nada: las migraciones se aplican al arrancar. Solo para **crear** una migración nueva se necesita la herramienta `dotnet-ef`, fijada en el manifiesto local:

```bash
dotnet tool restore
ConnectionStrings__Default="Host=localhost" dotnet ef migrations add <Nombre> \
  -p src/MaintenanceTracker.Infrastructure -s src/MaintenanceTracker.Api -o Persistence/Migrations
```

La cadena de conexión es ficticia: la API exige una al arrancar, pero crear la migración no se conecta a la base.

## Configuración

Todo se configura con variables de entorno en `.env`:

| Variable | Uso | Por defecto |
|---|---|---|
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` | Credenciales de la base (solo desarrollo) | ver `.env.example` |
| `POSTGRES_PORT` | Puerto de PostgreSQL en el host | `5432` |
| `API_PORT` | Puerto de la API en el host | `8080` |
| `DATABASE_APPLY_MIGRATIONS_ON_STARTUP` | Aplicar migraciones al arrancar | `true` |
| `CORS_ALLOWED_ORIGINS` | Orígenes permitidos, separados por comas | `http://localhost:3000` |

## Arquitectura

```
src/
  MaintenanceTracker.Domain          agregado, máquina de estados, historial; sin dependencias
  MaintenanceTracker.Application     casos de uso, modelos de respuesta, interfaces de persistencia
  MaintenanceTracker.Infrastructure  EF Core + PostgreSQL, migraciones, repositorio y consultas
  MaintenanceTracker.Api             controladores, manejo de errores, composición
tests/
  MaintenanceTracker.Domain.Tests
  MaintenanceTracker.Api.IntegrationTests
```

`Domain` no referencia nada y `Application` solo a `Domain`, así que el compilador impide que EF Core o ASP.NET se filtren al modelo. Los controladores usan únicamente casos de uso de `Application`.

Las decisiones están documentadas y numeradas:

- [`docs/domain/event-storming.md`](docs/domain/event-storming.md): lenguaje del dominio, reglas y decisiones D1–D12 (por qué `InProgress` exige responsable, por qué el historial es parte del agregado, cuándo 400 y cuándo 409, etc.).
- [`docs/architecture/tactical-design.md`](docs/architecture/tactical-design.md): decisiones de implementación T1–T34 (capas, excepciones frente a Result, CQRS sin librerías, TPH para el historial, `xmin` como token de concurrencia, etc.).

Las más importantes, en corto:

- **El historial es parte del agregado.** Cada operación valida su regla y agrega su entrada en el mismo `SaveChanges`: no puede haber un cambio sin su entrada, ni una entrada de un intento rechazado.
- **Una sola tabla de transiciones** (`StatusTransitions`), que usan el agregado para validar y la consulta del detalle para calcular `allowedTransitions`. El frontend no replica las reglas.
- **Concurrencia optimista** con la columna de sistema `xmin` de PostgreSQL: si dos usuarios cambian la misma solicitud, el segundo recibe 409 `concurrency_conflict` en lugar de pisar al primero.
- **Lecturas y escrituras separadas:** las escrituras cargan el agregado por el repositorio; las lecturas proyectan directamente a los modelos de respuesta sin cargar agregados.
- **Sin MediatR, AutoMapper ni FluentValidation:** con este tamaño agregan indirección sin beneficio. Las reglas viven en el dominio y la API solo valida la forma de la petición.

## Limitaciones y fuera de alcance

- **Migraciones al arrancar.** Cumple con "clonar y ejecutar", pero en un entorno real se aplicarían desde el pipeline de despliegue. Se desactivan con `DATABASE_APPLY_MIGRATIONS_ON_STARTUP=false`.
- **Sin autenticación ni roles.** El usuario llega en `X-User-Id` y se lee en un solo punto (`ICurrentUserProvider`); con autenticación real solo cambiaría esa clase.
- **Fuera de alcance:** desasignar responsable, editar título o descripción, eliminar solicitudes, notificaciones y despliegue en la nube.
