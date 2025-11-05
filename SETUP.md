# 🚀 Guía de Configuración Rápida - Star Wars API

## Inicio Rápido con Docker (5 minutos)

### Paso 1: Verificar Requisitos

```bash
# Verificar Docker
docker --version
docker-compose --version

# Verificar .NET SDK (opcional, solo si vas a desarrollar)
dotnet --version
```

### Paso 2: Iniciar la Aplicación

```bash
# Clonar el repositorio (si aplica)
git clone <repository-url>
cd StarWars

# Iniciar todos los servicios
docker-compose up -d

# Ver los logs en tiempo real
docker-compose logs -f starwars-api
```

### Paso 3: Verificar que Todo Funcione

```bash
# Health Check
curl http://localhost:5000/health

# Debería retornar: {"status": "Healthy", ...}
```

### Paso 4: Probar la API

Abrir en el navegador: http://localhost:5000

Verás la interfaz de Swagger donde puedes probar todos los endpoints.

## Pruebas Rápidas desde la Terminal

```bash
# 1. Listar personajes
curl http://localhost:5000/api/v1/characters

# 2. Buscar a Luke Skywalker
curl "http://localhost:5000/api/v1/characters/search?name=Luke"

# 3. Obtener detalles del personaje con ID 1
curl http://localhost:5000/api/v1/characters/1

# 4. Agregar a favoritos
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1", "notes": "Mi favorito"}'

# 5. Ver favoritos
curl http://localhost:5000/api/v1/favorites

# 6. Ver historial de peticiones
curl http://localhost:5000/api/v1/history

# 7. Ver estadísticas
curl http://localhost:5000/api/v1/history/statistics
```

## Usar el Cliente de Consola

```bash
# Opción 1: Con Docker
docker-compose run --rm starwars-client

# Opción 2: Ejecutar localmente
cd src/StarWars.Client
dotnet run
```

Sigue las instrucciones en pantalla para navegar por el menú interactivo.

## Detener la Aplicación

```bash
# Detener los servicios
docker-compose down

# Detener y eliminar todos los datos (limpieza completa)
docker-compose down -v
```

## Solución de Problemas Comunes

### El puerto 5000 ya está en uso

```bash
# Editar docker-compose.yml y cambiar el puerto:
# ports:
#   - "5001:8080"  # Usar 5001 en lugar de 5000
```

### La API no responde

```bash
# Ver logs
docker-compose logs starwars-api

# Reiniciar el contenedor
docker-compose restart starwars-api
```

### La base de datos no se conecta

```bash
# Verificar que PostgreSQL esté corriendo
docker-compose ps

# Reiniciar PostgreSQL
docker-compose restart postgres
```

### Limpiar y empezar de nuevo

```bash
# Detener todo
docker-compose down -v

# Eliminar imágenes
docker rmi starwars-starwars-api

# Reconstruir todo
docker-compose build --no-cache
docker-compose up -d
```

## Desarrollo Local (sin Docker)

Si prefieres ejecutar la aplicación sin Docker:

### 1. Instalar PostgreSQL

```bash
# En Ubuntu/Debian
sudo apt-get install postgresql-16

# En macOS con Homebrew
brew install postgresql@16

# En Windows
# Descargar desde: https://www.postgresql.org/download/windows/
```

### 2. Crear la Base de Datos

```sql
-- Conectarse a PostgreSQL
psql -U postgres

-- Crear base de datos y usuario
CREATE DATABASE starwarsdb;
CREATE USER starwars WITH PASSWORD 'starwars123';
GRANT ALL PRIVILEGES ON DATABASE starwarsdb TO starwars;
```

### 3. Configurar la Aplicación

Editar `src/StarWars.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=starwarsdb;Username=starwars;Password=starwars123"
  }
}
```

### 4. Ejecutar la API

```bash
cd src/StarWars.Api
dotnet restore
dotnet run
```

### 5. Ejecutar el Cliente

```bash
cd src/StarWars.Client
dotnet run
```

## Endpoints Disponibles

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | /health | Health check |
| GET | /api/v1/characters | Listar personajes (paginado) |
| GET | /api/v1/characters/{id} | Obtener personaje por ID |
| GET | /api/v1/characters/search?name={name} | Buscar personajes |
| GET | /api/v1/favorites | Listar favoritos |
| POST | /api/v1/favorites | Agregar favorito |
| DELETE | /api/v1/favorites/{id} | Eliminar favorito |
| GET | /api/v1/history | Ver historial |
| GET | /api/v1/history/statistics | Ver estadísticas |

## Documentación Completa

Para más detalles, consulta el [README.md](README.md) completo.

## Contacto y Soporte

Si tienes algún problema, revisa los logs:

```bash
docker-compose logs -f
```

---

**¡Listo para usar! May the Force be with you!** 🌟

