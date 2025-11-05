# 🌟 Star Wars API Microservice

Microservicio desarrollado en .NET 8 que integra con la Star Wars API (SWAPI) y proporciona funcionalidad adicional para gestionar personajes favoritos, historial de peticiones y caché.

## 📋 Tabla de Contenidos

- [Características](#características)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Arquitectura](#arquitectura)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Uso de la API](#uso-de-la-api)
- [Cliente de Consola](#cliente-de-consola)
- [Documentación API](#documentación-api)
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
- 🔒 **Manejo de Errores**: Sistema centralizado de manejo de errores

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
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL 16](https://www.postgresql.org/download/) (opcional, si no usa Docker)

## 🚀 Instalación y Ejecución

### Opción 1: Usando Docker (Recomendado)

1. **Clonar el repositorio**
```bash
git clone <repository-url>
cd StarWars
```

2. **Iniciar los servicios con Docker Compose**
```bash
docker-compose up -d
```

3. **Verificar que los servicios estén corriendo**
```bash
docker-compose ps
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

Edita `src/StarWars.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=starwarsdb;Username=starwars;Password=starwars123"
  }
}
```

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

## 🧪 Testing

### Ejecutar Tests Manualmente

Puedes probar la API usando los siguientes comandos:

```bash
# Health Check
curl http://localhost:5000/health

# Listar primeros personajes
curl http://localhost:5000/api/v1/characters?page=1

# Buscar "Luke"
curl "http://localhost:5000/api/v1/characters/search?name=Luke"

# Agregar a favoritos
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1", "notes": "El elegido"}'

# Ver favoritos
curl http://localhost:5000/api/v1/favorites

# Ver historial
curl http://localhost:5000/api/v1/history?limit=10
```

### Verificar Rate Limiting

```bash
# Ejecutar múltiples peticiones rápidamente
for i in {1..65}; do 
  curl http://localhost:5000/api/v1/characters
done

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

### 4. Logging y Métricas

- Registro automático de todas las peticiones
- Tiempo de respuesta
- Códigos de estado
- Estadísticas de endpoints más utilizados

## 🔧 Configuración

### Variables de Entorno

Puedes configurar la aplicación usando variables de entorno:

```bash
# Cadena de conexión
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=starwarsdb;Username=starwars;Password=starwars123"

# Entorno
export ASPNETCORE_ENVIRONMENT=Production

# URL de escucha
export ASPNETCORE_URLS="http://+:8080"
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

```bash
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

## 🐳 Docker

### Comandos Útiles

```bash
# Iniciar servicios
docker-compose up -d

# Ver logs
docker-compose logs -f starwars-api

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes (limpieza completa)
docker-compose down -v

# Reconstruir imágenes
docker-compose build --no-cache

# Verificar estado
docker-compose ps
```

### Acceso a la Base de Datos

```bash
# Conectarse a PostgreSQL desde el contenedor
docker exec -it starwars-postgres psql -U starwars -d starwarsdb

# O usando un cliente externo
psql -h localhost -p 5432 -U starwars -d starwarsdb
```

## 📝 Notas Adicionales

### Rendimiento

- Caché implementada para reducir llamadas a SWAPI
- Índices en base de datos para queries optimizadas
- Rate limiting para proteger contra abuso
- Connection pooling en Entity Framework

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

