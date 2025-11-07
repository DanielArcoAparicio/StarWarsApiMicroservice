# 🌟 Star Wars API Microservice

Microservicio desarrollado en .NET 8 que integra con la Star Wars API (SWAPI) y proporciona funcionalidad adicional para gestionar personajes favoritos, historial de peticiones y caché.

## 📋 Tabla de Contenidos

- [Características](#características)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Arquitectura](#arquitectura)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Debugging en Visual Studio](#debugging-en-visual-studio)
- [Uso de la API](#uso-de-la-api)
- [Cliente de Consola](#cliente-de-consola)
- [Documentación API](#documentación-api)
- [Docker](#docker)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Características Bonus](#características-bonus)

## ✨ Características

### Características Principales

- ✅ **Integración completa con SWAPI**: Acceso a personajes, planetas, películas, especies, vehículos y naves
- ✅ **Gestión de Favoritos**: Guardar y administrar personajes favoritos
- ✅ **Historial de Peticiones**: Tracking completo de todas las peticiones realizadas
- ✅ **Sistema de Caché Multinivel**: Caché en memoria y persistente en base de datos
- ✅ **API REST con versionado**: Endpoints RESTful siguiendo mejores prácticas
- ✅ **Cliente de Consola Interactivo**: Aplicación de consola con menú completo

### Características Bonus Implementadas

- 🚀 **Rate Limiting**: Límite de peticiones por IP (60/min, 1000/hora)
- 💚 **Health Checks**: Monitoreo de salud de la API y dependencias
- 📊 **Métricas y Estadísticas**: Estadísticas de uso de endpoints
- 📝 **Documentación Swagger/OpenAPI**: Documentación interactiva completa
- ⚡ **Optimización de Performance**: Caché inteligente y queries optimizadas
- 🔒 **Manejo de Errores**: Sistema centralizado de manejo de errores robusto

## 🛠️ Tecnologías Utilizadas

- **Framework**: .NET 8
- **Base de Datos**: PostgreSQL 16
- **ORM**: Entity Framework Core 8
- **Containerización**: Docker & Docker Compose
- **API Externa**: SWAPI (https://swapi.dev/api)
- **Caché**: Memory Cache + Database Cache
- **Rate Limiting**: AspNetCoreRateLimit
- **Documentación**: Swagger/OpenAPI

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** con las siguientes capas:

```
StarWars/
├── src/
│   ├── StarWars.Domain/          # Entidades y modelos del dominio
│   ├── StarWars.Application/     # Interfaces y lógica de negocio
│   ├── StarWars.Infrastructure/  # Implementaciones (EF Core, SWAPI Client)
│   ├── StarWars.Api/            # API REST (controladores, middleware)
│   └── StarWars.Client/         # Aplicación de consola
├── scripts/                      # Scripts de base de datos
├── Dockerfile                    # Configuración Docker para la API
├── docker-compose.yml           # Orquestación de servicios
└── README.md                    # Este archivo
```

### Componentes Principales

1. **StarWars.Domain**: Entidades de dominio (FavoriteCharacter, ApiRequestHistory, CachedData)
2. **StarWars.Application**: Interfaces de servicios y DTOs
3. **StarWars.Infrastructure**: Implementación de servicios, DbContext, cliente SWAPI
4. **StarWars.Api**: Controladores REST, middleware, configuración
5. **StarWars.Client**: Cliente de consola interactivo

## 📦 Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (recomendado)
- [PostgreSQL 16](https://www.postgresql.org/download/) (opcional, si no usa Docker)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (para debugging)

## 🚀 Instalación y Ejecución

### Opción 1: Usando Docker (Recomendado)

1. **Clonar el repositorio**
```bash
git clone <repository-url>
cd SWP
```

2. **Iniciar los servicios con Docker Compose**
```powershell
# Nota: En versiones recientes de Docker Desktop, el comando es 'docker compose' (sin guión)
docker compose up -d
```

3. **Verificar que los servicios estén corriendo**
```powershell
docker compose ps
```

4. **Acceder a la API**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000
- Health Check: http://localhost:5000/health

### Opción 2: Ejecución Local

1. **Configurar PostgreSQL**

Asegúrate de tener PostgreSQL corriendo y crear la base de datos:

```sql
CREATE DATABASE starwarsdb;
CREATE USER starwars WITH PASSWORD 'starwars123';
GRANT ALL PRIVILEGES ON DATABASE starwarsdb TO starwars;
```

2. **Configurar la cadena de conexión**

Edita `src/StarWars.Api/appsettings.json` y `src/StarWars.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=starwarsdb;Username=starwars;Password=starwars123"
  }
}
```

**Importante**: `appsettings.Development.json` debe usar `Host=localhost` (no `postgres`) para ejecución local desde Visual Studio.

3. **Restaurar dependencias y ejecutar**

```bash
# Restaurar paquetes
dotnet restore

# Aplicar migraciones (se hace automáticamente al iniciar la API)
cd src/StarWars.Api
dotnet ef database update

# Ejecutar la API
dotnet run
```

4. **Ejecutar el Cliente de Consola**

En otra terminal:

```bash
cd src/StarWars.Client
dotnet run
```

## 🔍 Debugging en Visual Studio

### Inicio Rápido

1. **Preparar PostgreSQL**
   - **Docker**: `docker compose up -d postgres`
   - **Local**: Asegúrate de que PostgreSQL esté corriendo en el puerto 5432

2. **Abrir la solución**
   - Abre `StarWars.sln` en Visual Studio 2022
   - Espera a que se restauren los paquetes NuGet

3. **Configurar proyecto de inicio**
   - Clic derecho en `StarWars.Api` → **Set as Startup Project**

4. **Iniciar debugging**
   - Presiona **F5**
   - Se abrirá Swagger en `http://localhost:5000`

5. **Establecer breakpoints**
   - Haz clic en el margen izquierdo del editor para agregar breakpoints

### Configuración Múltiple (API + Cliente)

Para debuggear ambos proyectos simultáneamente:

1. Clic derecho en la **Solución** → **Properties**
2. Selecciona **Multiple startup projects**
3. Configura:
   - **StarWars.Api**: `Start`
   - **StarWars.Client**: `Start`
4. Presiona **F5**

### Controles de Debugging

| Tecla | Acción |
|-------|--------|
| **F5** | Iniciar/Continuar debugging |
| **F9** | Agregar/Quitar breakpoint |
| **F10** | Step Over (siguiente línea) |
| **F11** | Step Into (entrar en método) |
| **Shift+F11** | Step Out (salir del método) |
| **Shift+F5** | Detener debugging |

### Ventanas de Debugging

Abre desde **Debug → Windows**:

- **Autos** (`Ctrl+Alt+V, A`): Variables automáticas
- **Locals** (`Ctrl+Alt+V, L`): Variables locales
- **Watch** (`Ctrl+Alt+W, 1`): Expresiones personalizadas
- **Call Stack** (`Ctrl+Alt+C`): Pila de llamadas
- **Output** (`Ctrl+Alt+O`): Logs y salida

### Ubicaciones Recomendadas para Breakpoints

**CharactersController.cs**:
```csharp
[HttpGet]
public async Task<IActionResult> GetCharacters([FromQuery] int page = 1)
{
    // ← Punto de interrupción aquí
    var result = await _swapiService.GetCharactersAsync(page);
    return Ok(result);
}
```

**FavoritesController.cs**:
```csharp
[HttpPost]
public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
{
    // ← Punto de interrupción aquí
    var favorite = await _favoriteService.AddFavoriteAsync(...);
    return CreatedAtAction(...);
}
```

## 📖 Uso de la API

### Endpoints Principales

#### Personajes (Characters)

```http
# Obtener lista de personajes (paginado)
GET /api/v1/characters?page=1

# Buscar personajes por nombre
GET /api/v1/characters/search?name=Luke

# Obtener detalles de un personaje
GET /api/v1/characters/{id}
```

#### Favoritos (Favorites)

```http
# Obtener todos los favoritos
GET /api/v1/favorites

# Agregar personaje a favoritos
POST /api/v1/favorites
Content-Type: application/json

{
  "characterId": "1",
  "notes": "Mi personaje favorito"
}

# Eliminar favorito por ID
DELETE /api/v1/favorites/{id}

# Eliminar favorito por SWAPI ID
DELETE /api/v1/favorites/character/{swapiId}
```

#### Historial (History)

```http
# Obtener historial de peticiones
GET /api/v1/history?limit=100

# Obtener estadísticas
GET /api/v1/history/statistics
```

### Ejemplos con cURL

```bash
# Listar personajes
curl http://localhost:5000/api/v1/characters

# Buscar personaje
curl "http://localhost:5000/api/v1/characters/search?name=Vader"

# Agregar favorito
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1", "notes": "Luke Skywalker"}'

# Ver favoritos
curl http://localhost:5000/api/v1/favorites

# Ver historial
curl http://localhost:5000/api/v1/history

# Health check
curl http://localhost:5000/health
```

## 🖥️ Cliente de Consola

El cliente de consola proporciona una interfaz interactiva para utilizar todas las funcionalidades de la API.

### Características del Cliente

1. **Listar personajes**: Navegación paginada
2. **Buscar personajes**: Búsqueda por nombre
3. **Ver detalles**: Información completa de un personaje
4. **Gestionar favoritos**: Agregar/eliminar favoritos
5. **Ver historial**: Últimas peticiones realizadas
6. **Estadísticas**: Top endpoints más utilizados

### Ejecutar el Cliente

```bash
cd src/StarWars.Client
dotnet run
```

Al iniciar, se te pedirá la URL de la API (por defecto: http://localhost:5000).

## 📚 Documentación API

### Swagger/OpenAPI

La documentación interactiva está disponible en:

- **Swagger UI**: http://localhost:5000
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

### Estructura de Respuestas

#### Respuesta Exitosa (Personajes)

```json
{
  "count": 82,
  "next": "https://swapi.dev/api/people/?page=2",
  "previous": null,
  "results": [
    {
      "id": "1",
      "name": "Luke Skywalker",
      "height": "172",
      "mass": "77",
      "hairColor": "blond",
      "skinColor": "fair",
      "eyeColor": "blue",
      "birthYear": "19BBY",
      "gender": "male",
      "homeWorld": "https://swapi.dev/api/planets/1/",
      "films": [...],
      "species": [...],
      "vehicles": [...],
      "starships": [...],
      "url": "https://swapi.dev/api/people/1/",
      "isFavorite": false
    }
  ],
  "page": 1,
  "totalPages": 9
}
```

#### Respuesta de Error

```json
{
  "error": {
    "message": "Personaje con ID 999 no encontrado",
    "type": "KeyNotFoundException",
    "statusCode": 404
  }
}
```

## 🐳 Docker

### Comandos Básicos

```powershell
# Iniciar servicios
docker compose up -d

# Ver logs
docker compose logs -f starwars-api

# Detener servicios
docker compose down

# Verificar estado
docker compose ps
```

### Reiniciar con Cambios de Código

Si hiciste cambios en el código y necesitas aplicarlos:

```powershell
# 1. Detener y eliminar los contenedores
docker compose down

# 2. Reconstruir la imagen de la API (con los cambios)
docker compose build --no-cache starwars-api

# 3. Iniciar los servicios nuevamente
docker compose up -d

# 4. Ver los logs para verificar que todo esté bien
docker compose logs -f starwars-api
```

### Reinicio Rápido (Sin Cambios de Código)

```powershell
# Reiniciar solo la API
docker compose restart starwars-api

# O reiniciar todos los servicios
docker compose restart
```

**⚠️ Nota Importante**: Si hiciste cambios en el código, **debes reconstruir la imagen** con `docker compose build` para que los cambios se apliquen. Si solo reinicias con `docker compose restart`, los cambios **NO se aplicarán**.

### Acceso a la Base de Datos

```powershell
# Conectarse a PostgreSQL desde el contenedor
docker compose exec postgres psql -U starwars -d starwarsdb

# O usando un cliente externo
psql -h localhost -p 5432 -U starwars -d starwarsdb
```

## 🐛 Troubleshooting

### Problema: "Docker no se reconoce como comando"

**Solución**: Docker no está instalado o no está en el PATH

1. **Instalar Docker Desktop**:
   - Descarga desde: https://www.docker.com/products/docker-desktop
   - Instala y reinicia tu computadora
   - Verifica: `docker --version`

2. **Alternativa - Usar PostgreSQL Local**:
   - Instala PostgreSQL 16 desde: https://www.postgresql.org/download/windows/
   - Crea la base de datos:
     ```sql
     CREATE DATABASE starwarsdb;
     CREATE USER starwars WITH PASSWORD 'starwars123';
     GRANT ALL PRIVILEGES ON DATABASE starwarsdb TO starwars;
     ```

### Problema: "Error 500 en Docker Engine"

**Solución**: Docker Desktop necesita reiniciarse

1. Cierra Docker Desktop completamente (Quit Docker Desktop)
2. Espera 10-15 segundos
3. Reinicia Docker Desktop
4. Verifica: `docker info` (debería mostrar información sin errores)

### Problema: "Error 500 al descargar imagen de PostgreSQL"

**Solución**: Problema con Docker Desktop o conexión

1. **Reinicia Docker Desktop** (más común)
2. **Descarga la imagen manualmente**: `docker pull postgres:16-alpine`
3. **Verifica conexión a Internet** y acceso a Docker Hub

### Problema: "Host desconocido" al conectar a PostgreSQL

**Causa**: `appsettings.Development.json` tiene `Host=postgres` (para Docker) pero ejecutas desde Visual Studio

**Solución**: Verifica que `appsettings.Development.json` tenga:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=starwarsdb;Username=starwars;Password=starwars123"
  }
}
```

### Problema: "Error 500 al listar/buscar/obtener personajes"

**Causas posibles**:
1. PostgreSQL no está corriendo
2. SWAPI no está disponible
3. Problema con la base de datos

**Soluciones**:
1. Verifica PostgreSQL: `docker ps` o `Get-Service -Name postgresql*`
2. Verifica SWAPI: Abre https://swapi.dev/api/people/ en el navegador
3. Revisa los logs en Visual Studio Output (`Ctrl+Alt+O`)
4. Prueba el endpoint directamente: `http://localhost:5000/api/v1/characters?page=1`

### Problema: "Error 500 al agregar favoritos" o "Tabla FavoriteCharacters no existe"

**Causa**: Las migraciones no se aplicaron correctamente

**Solución**:
1. Verifica que las tablas existan:
   ```powershell
   docker compose exec postgres psql -U starwars -d starwarsdb -c '\dt'
   ```
2. Si faltan tablas, reinicia el contenedor para que se apliquen las migraciones automáticamente:
   ```powershell
   docker compose restart starwars-api
   ```
3. O aplica las migraciones manualmente si es necesario

### Problema: "Puerto 5000 ya está en uso"

**Solución**:
```powershell
# Encuentra el proceso
netstat -ano | findstr :5000

# Detén el proceso (reemplaza <PID> con el número)
taskkill /PID <PID> /F
```

### Problema: "Los breakpoints no se activan"

**Solución**:
1. Verifica que estés en modo **Debug** (no Release)
2. **Build → Rebuild Solution**
3. Limpia: **Build → Clean Solution** → **Build → Rebuild Solution**

### Problema: "El Cliente no puede conectar a la API"

**Solución**:
1. Verifica que la API esté corriendo: `http://localhost:5000/health`
2. Revisa los logs en la ventana **Output** de Visual Studio
3. Asegúrate de que el puerto 5000 esté disponible

## 🧪 Testing

### Tests Unitarios

El proyecto incluye una suite completa de tests unitarios usando **xUnit**, **Moq** y **FluentAssertions**.

#### Ejecutar Tests Localmente

```powershell
# Ejecutar todos los tests
dotnet test tests/StarWars.Tests/StarWars.Tests.csproj

# Ejecutar tests con cobertura
dotnet test tests/StarWars.Tests/StarWars.Tests.csproj --collect:"XPlat Code Coverage"

# Ejecutar tests con salida detallada
dotnet test tests/StarWars.Tests/StarWars.Tests.csproj --verbosity normal
```

#### Estructura de Tests

```
tests/StarWars.Tests/
├── Controllers/
│   ├── CharactersControllerTests.cs
│   ├── FavoritesControllerTests.cs
│   └── HistoryControllerTests.cs
└── Services/
    ├── SwapiClientTests.cs
    ├── FavoriteCharacterServiceTests.cs
    └── CacheServiceTests.cs
```

#### Cobertura de Código

El proyecto está configurado para generar reportes de cobertura automáticamente en GitHub Actions. El reporte se publica en **GitHub Pages** después de cada push a `main`.

**Ver el reporte de cobertura:**
- GitHub Pages: `https://[TU-USUARIO].github.io/[NOMBRE-REPO]/coverage/`
- Codecov: Se sube automáticamente si tienes una cuenta configurada

### Tests Manuales de la API

Puedes probar la API usando los siguientes comandos:

```powershell
# Health Check
Invoke-WebRequest -Uri "http://localhost:5000/health"

# Listar primeros personajes
Invoke-WebRequest -Uri "http://localhost:5000/api/v1/characters?page=1"

# Buscar "Luke"
Invoke-WebRequest -Uri "http://localhost:5000/api/v1/characters/search?name=Luke"

# Agregar a favoritos
$body = @{ CharacterId = "1"; Notes = "El elegido" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://localhost:5000/api/v1/favorites" -Method POST -Body $body -ContentType "application/json"

# Ver favoritos
Invoke-WebRequest -Uri "http://localhost:5000/api/v1/favorites"

# Ver historial
Invoke-WebRequest -Uri "http://localhost:5000/api/v1/history?limit=10"
```

### Verificar Rate Limiting

```powershell
# Ejecutar múltiples peticiones rápidamente
1..65 | ForEach-Object {
    Invoke-WebRequest -Uri "http://localhost:5000/api/v1/characters"
}

# La petición 61+ debería retornar 429 (Too Many Requests)
```

## 🎯 Características Bonus

### 1. Rate Limiting

- **Límite por minuto**: 60 peticiones
- **Límite por hora**: 1000 peticiones
- **Status Code**: 429 Too Many Requests cuando se excede

### 2. Health Checks

Endpoint: `/health`

Verifica:
- Conexión a PostgreSQL
- Disponibilidad de SWAPI
- Estado general de la aplicación

### 3. Sistema de Caché

- **Nivel 1**: Memory Cache (In-Memory) - Más rápido
- **Nivel 2**: Database Cache - Persistente
- **TTL Configurable**: Por defecto 30 min para listados, 1 hora para detalles
- **Estadísticas**: Tracking de accesos y hits
- **Resiliente**: Continúa funcionando aunque falle la BD (solo memoria)

### 4. Logging y Métricas

- Registro automático de todas las peticiones
- Tiempo de respuesta
- Códigos de estado
- Estadísticas de endpoints más utilizados

### 5. Manejo de Errores Robusto

- Manejo centralizado de excepciones
- Mensajes de error claros y descriptivos
- Resiliencia: la aplicación continúa funcionando aunque algunos servicios fallen
- Logging detallado para debugging

## 🔧 Configuración

### Variables de Entorno

Puedes configurar la aplicación usando variables de entorno:

```powershell
# Cadena de conexión
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=starwarsdb;Username=starwars;Password=starwars123"

# Entorno
$env:ASPNETCORE_ENVIRONMENT="Production"

# URL de escucha
$env:ASPNETCORE_URLS="http://+:8080"
```

### Configuración de Rate Limiting

Edita `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ]
  }
}
```

## 🗄️ Base de Datos

### Migraciones

Las migraciones se aplican automáticamente al iniciar la API. Si necesitas crearlas manualmente:

```powershell
cd src/StarWars.Api

# Crear una nueva migración
dotnet ef migrations add MigrationName

# Aplicar migraciones
dotnet ef database update

# Revertir a una migración anterior
dotnet ef database update PreviousMigrationName
```

### Esquema de Base de Datos

#### Tabla: FavoriteCharacters

- `Id` (PK): int
- `SwapiId`: string (unique)
- `Name`: string
- `Gender`: string
- `BirthYear`: string
- `HomeWorld`: string
- `AddedDate`: datetime
- `Notes`: string (nullable)

#### Tabla: RequestHistory

- `Id` (PK): int
- `Endpoint`: string
- `Method`: string
- `QueryParameters`: string (nullable)
- `StatusCode`: int
- `RequestDate`: datetime
- `ResponseTimeMs`: long
- `ErrorMessage`: string (nullable)
- `IpAddress`: string (nullable)

#### Tabla: CachedData

- `Id` (PK): int
- `CacheKey`: string (unique)
- `Data`: string (JSON)
- `CreatedDate`: datetime
- `ExpirationDate`: datetime
- `AccessCount`: int
- `LastAccessDate`: datetime

## 📝 Notas Adicionales

### Rendimiento

- Caché implementada para reducir llamadas a SWAPI
- Índices en base de datos para queries optimizadas
- Rate limiting para proteger contra abuso
- Connection pooling en Entity Framework
- Manejo de errores que permite continuar funcionando aunque algunos servicios fallen

### Seguridad

- Validación de entrada en todos los endpoints
- Manejo centralizado de errores
- Rate limiting por IP
- Headers de seguridad configurados

### Escalabilidad

- Arquitectura preparada para múltiples instancias
- Caché distribuida fácil de implementar
- Base de datos PostgreSQL con soporte para réplicas
- Docker para despliegue en cualquier plataforma

## 🤝 Contribución

Si deseas contribuir al proyecto:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 🔄 CI/CD

El proyecto incluye **GitHub Actions** para automatizar:

- ✅ **Ejecución automática de tests** en cada push y PR
- 📊 **Generación de reportes de cobertura** de código
- 📄 **Publicación automática en GitHub Pages** del reporte de cobertura
- 📤 **Integración con Codecov** (opcional)

### Configurar GitHub Pages

Para habilitar la publicación automática del reporte de cobertura:

1. Ve a **Settings** → **Pages** en tu repositorio
2. En **Source**, selecciona **GitHub Actions**
3. Guarda los cambios

El workflow se ejecutará automáticamente en cada push a `main` y publicará el reporte en:
`https://[TU-USUARIO].github.io/[NOMBRE-REPO]/coverage/`

Para más información, consulta [.github/workflows/README.md](.github/workflows/README.md)

## 📄 Licencia

Este proyecto fue desarrollado como prueba técnica.

## 👤 Autor

Desarrollado como parte de una prueba técnica para integración con Star Wars API.

## 🌟 Agradecimientos

- Star Wars API (SWAPI): https://swapi.dev
- .NET Team por el excelente framework
- Comunidad Open Source

---

**¡Que la Fuerza te acompañe!** 🚀
