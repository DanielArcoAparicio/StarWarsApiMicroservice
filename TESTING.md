# 🧪 Guía de Testing - Star Wars API

## Índice

1. [Pruebas Manuales](#pruebas-manuales)
2. [Pruebas con cURL](#pruebas-con-curl)
3. [Pruebas con Cliente de Consola](#pruebas-con-cliente-de-consola)
4. [Verificación de Características](#verificación-de-características)
5. [Casos de Prueba](#casos-de-prueba)

## Pruebas Manuales

### 1. Verificar que la Aplicación Está Corriendo

```bash
# Health Check
curl http://localhost:5000/health

# Respuesta esperada:
# {"status":"Healthy","results":{...}}
```

### 2. Acceder a Swagger UI

Abrir en el navegador: http://localhost:5000

Deberías ver la interfaz interactiva de Swagger con todos los endpoints disponibles.

## Pruebas con cURL

### Escenario 1: Explorar Personajes

```bash
# 1. Obtener la primera página de personajes
curl http://localhost:5000/api/v1/characters?page=1

# 2. Obtener la segunda página
curl http://localhost:5000/api/v1/characters?page=2

# 3. Buscar personajes por nombre
curl "http://localhost:5000/api/v1/characters/search?name=Luke"

# 4. Obtener detalles de un personaje específico
curl http://localhost:5000/api/v1/characters/1
```

**Verificar:**
- ✅ Los personajes se listan correctamente
- ✅ La paginación funciona
- ✅ La búsqueda retorna resultados relevantes
- ✅ Los detalles incluyen toda la información

### Escenario 2: Gestión de Favoritos

```bash
# 1. Ver favoritos actuales (debería estar vacío inicialmente)
curl http://localhost:5000/api/v1/favorites

# 2. Agregar Luke Skywalker a favoritos
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1", "notes": "El héroe principal"}'

# 3. Agregar Darth Vader a favoritos
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "4", "notes": "El villano icónico"}'

# 4. Verificar que se agregaron
curl http://localhost:5000/api/v1/favorites

# 5. Buscar a Luke de nuevo y verificar que aparece marcado como favorito
curl http://localhost:5000/api/v1/characters/1

# 6. Eliminar un favorito (usar el ID del response anterior)
curl -X DELETE http://localhost:5000/api/v1/favorites/1

# 7. Verificar que se eliminó
curl http://localhost:5000/api/v1/favorites
```

**Verificar:**
- ✅ Los favoritos se agregan correctamente
- ✅ Los personajes se marcan como favoritos en las búsquedas
- ✅ Los favoritos se eliminan correctamente
- ✅ No se puede agregar el mismo favorito dos veces

### Escenario 3: Historial y Estadísticas

```bash
# 1. Ver historial de las últimas 20 peticiones
curl http://localhost:5000/api/v1/history?limit=20

# 2. Ver estadísticas de uso
curl http://localhost:5000/api/v1/history/statistics
```

**Verificar:**
- ✅ El historial registra todas las peticiones
- ✅ Se incluyen tiempos de respuesta
- ✅ Las estadísticas muestran los endpoints más usados

### Escenario 4: Caché

```bash
# Primera llamada (sin caché)
time curl http://localhost:5000/api/v1/characters/1

# Segunda llamada (con caché - debería ser más rápida)
time curl http://localhost:5000/api/v1/characters/1
```

**Verificar:**
- ✅ La segunda llamada es significativamente más rápida
- ✅ Los datos son idénticos en ambas llamadas

### Escenario 5: Rate Limiting

```bash
# Ejecutar 65 peticiones rápidamente
for i in {1..65}; do 
  curl -s http://localhost:5000/api/v1/characters > /dev/null
  echo "Request $i"
done

# A partir de la petición 61, deberías recibir:
# Status: 429 Too Many Requests
```

**Verificar:**
- ✅ Las primeras 60 peticiones funcionan normalmente
- ✅ A partir de la 61, se retorna 429
- ✅ Después de 1 minuto, las peticiones vuelven a funcionar

### Escenario 6: Manejo de Errores

```bash
# 1. Buscar personaje inexistente
curl http://localhost:5000/api/v1/characters/9999

# Debería retornar 404 con mensaje descriptivo

# 2. Agregar favorito con ID inválido
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "9999"}'

# Debería retornar 404 indicando que el personaje no existe

# 3. Búsqueda sin parámetro
curl "http://localhost:5000/api/v1/characters/search"

# Debería retornar 400 Bad Request
```

**Verificar:**
- ✅ Los errores retornan códigos HTTP apropiados
- ✅ Los mensajes de error son descriptivos
- ✅ La API maneja errores sin crashear

## Pruebas con Cliente de Consola

### Iniciar el Cliente

```bash
cd src/StarWars.Client
dotnet run
```

### Flujo de Prueba Completo

1. **Seleccionar opción 1**: Listar personajes
   - Ingresar página 1
   - Verificar que se muestran 10 personajes

2. **Seleccionar opción 2**: Buscar personaje
   - Buscar "Skywalker"
   - Verificar que aparecen Luke, Anakin, etc.

3. **Seleccionar opción 3**: Ver detalles
   - Ingresar ID: 1 (Luke Skywalker)
   - Verificar que se muestran todos los detalles

4. **Seleccionar opción 5**: Agregar a favoritos
   - Ingresar ID: 1
   - Agregar nota: "Mi favorito"
   - Verificar mensaje de éxito

5. **Seleccionar opción 4**: Ver favoritos
   - Verificar que Luke aparece en la lista

6. **Seleccionar opción 1** de nuevo: Listar personajes
   - Verificar que Luke ahora tiene la estrella (★)

7. **Seleccionar opción 7**: Ver historial
   - Verificar que todas las acciones anteriores están registradas

8. **Seleccionar opción 8**: Ver estadísticas
   - Verificar que se muestran los endpoints más usados

9. **Seleccionar opción 6**: Eliminar de favoritos
   - Seleccionar el ID del favorito
   - Verificar mensaje de éxito

## Verificación de Características

### ✅ Características Principales

- [ ] **Integración SWAPI**
  - [ ] Listar personajes funciona
  - [ ] Buscar personajes funciona
  - [ ] Obtener detalles funciona
  - [ ] Paginación funciona correctamente

- [ ] **Favoritos**
  - [ ] Agregar favorito funciona
  - [ ] Eliminar favorito funciona
  - [ ] Listar favoritos funciona
  - [ ] Marcado de favoritos en búsquedas funciona

- [ ] **Historial**
  - [ ] Se registran todas las peticiones
  - [ ] Se incluyen tiempos de respuesta
  - [ ] Estadísticas funcionan correctamente

### ✅ Características Bonus

- [ ] **Caché**
  - [ ] Caché en memoria funciona
  - [ ] Caché persistente funciona
  - [ ] TTL funciona correctamente

- [ ] **Rate Limiting**
  - [ ] Límite por minuto funciona (60 req/min)
  - [ ] Límite por hora funciona (1000 req/hora)
  - [ ] Retorna 429 cuando se excede

- [ ] **Health Checks**
  - [ ] Endpoint /health responde
  - [ ] Verifica PostgreSQL
  - [ ] Verifica SWAPI

- [ ] **Documentación**
  - [ ] Swagger UI funciona
  - [ ] Todos los endpoints están documentados
  - [ ] Ejemplos de requests están disponibles

## Casos de Prueba

### Caso 1: Usuario Nuevo Explora la API

1. Usuario abre Swagger UI
2. Prueba GET /characters
3. Ve la lista de personajes
4. Busca a su personaje favorito
5. Lo agrega a favoritos
6. Verifica que está en favoritos

**Resultado Esperado**: Usuario puede navegar y usar la API intuitivamente.

### Caso 2: Desarrollador Integra con la API

1. Desarrollador lee README.md
2. Inicia la aplicación con Docker
3. Prueba endpoints con cURL
4. Integra en su aplicación
5. Verifica que funciona con rate limiting

**Resultado Esperado**: Desarrollador puede integrar fácilmente.

### Caso 3: Usuario Busca Múltiples Personajes

1. Usuario busca "Skywalker"
2. Agrega Luke a favoritos
3. Busca "Leia"
4. Agrega Leia a favoritos
5. Busca "Vader"
6. Agrega Vader a favoritos
7. Ve su lista de favoritos

**Resultado Esperado**: Todos los favoritos se guardan correctamente.

### Caso 4: Aplicación Bajo Carga

1. Se ejecutan 100 peticiones simultáneas
2. Algunas son bloqueadas por rate limiting
3. Las demás se procesan correctamente
4. El historial registra todo

**Resultado Esperado**: La API maneja la carga apropiadamente.

## Scripts de Prueba Automatizados

### test-all.sh (Linux/Mac)

```bash
#!/bin/bash

echo "🧪 Iniciando pruebas de Star Wars API..."

# Test 1: Health Check
echo "\n1️⃣ Health Check..."
curl -s http://localhost:5000/health | jq .

# Test 2: Listar personajes
echo "\n2️⃣ Listar personajes..."
curl -s http://localhost:5000/api/v1/characters?page=1 | jq '.results[0:2]'

# Test 3: Buscar personaje
echo "\n3️⃣ Buscar Luke..."
curl -s "http://localhost:5000/api/v1/characters/search?name=Luke" | jq '.[0]'

# Test 4: Agregar favorito
echo "\n4️⃣ Agregar favorito..."
curl -s -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1", "notes": "Test"}' | jq .

# Test 5: Ver favoritos
echo "\n5️⃣ Ver favoritos..."
curl -s http://localhost:5000/api/v1/favorites | jq .

# Test 6: Ver historial
echo "\n6️⃣ Ver historial..."
curl -s http://localhost:5000/api/v1/history?limit=5 | jq '.[0:2]'

echo "\n✅ Pruebas completadas!"
```

### test-all.ps1 (Windows PowerShell)

```powershell
Write-Host "🧪 Iniciando pruebas de Star Wars API..." -ForegroundColor Cyan

Write-Host "`n1️⃣ Health Check..." -ForegroundColor Yellow
Invoke-RestMethod http://localhost:5000/health | ConvertTo-Json

Write-Host "`n2️⃣ Listar personajes..." -ForegroundColor Yellow
$chars = Invoke-RestMethod "http://localhost:5000/api/v1/characters?page=1"
$chars.results[0..1] | ConvertTo-Json

Write-Host "`n3️⃣ Buscar Luke..." -ForegroundColor Yellow
$luke = Invoke-RestMethod "http://localhost:5000/api/v1/characters/search?name=Luke"
$luke[0] | ConvertTo-Json

Write-Host "`n4️⃣ Agregar favorito..." -ForegroundColor Yellow
$body = @{characterId="1"; notes="Test"} | ConvertTo-Json
Invoke-RestMethod http://localhost:5000/api/v1/favorites -Method Post -Body $body -ContentType "application/json" | ConvertTo-Json

Write-Host "`n5️⃣ Ver favoritos..." -ForegroundColor Yellow
Invoke-RestMethod http://localhost:5000/api/v1/favorites | ConvertTo-Json

Write-Host "`n6️⃣ Ver historial..." -ForegroundColor Yellow
$history = Invoke-RestMethod "http://localhost:5000/api/v1/history?limit=5"
$history[0..1] | ConvertTo-Json

Write-Host "`n✅ Pruebas completadas!" -ForegroundColor Green
```

## Resultados Esperados

Al completar todas las pruebas, deberías verificar:

1. ✅ Todos los endpoints responden correctamente
2. ✅ El caché mejora el rendimiento
3. ✅ Rate limiting protege la API
4. ✅ Los errores se manejan apropiadamente
5. ✅ El historial registra todas las operaciones
6. ✅ Los favoritos persisten en la base de datos
7. ✅ La documentación es clara y útil
8. ✅ El cliente de consola funciona correctamente

---

**¿Encontraste algún problema? Revisa los logs:**

```bash
docker-compose logs -f starwars-api
```

