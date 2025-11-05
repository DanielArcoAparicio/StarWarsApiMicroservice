# ⚡ Quick Start - Star Wars API

## 🎯 Objetivo

Poner en marcha la aplicación **en menos de 5 minutos**.

## 📋 Prerrequisitos

- Docker Desktop instalado y corriendo
- Git (opcional, si vas a clonar el repo)

## 🚀 Pasos

### 1. Obtener el Código

```bash
# Si tienes Git
git clone <repository-url>
cd StarWars

# Si descargaste el ZIP
unzip StarWars.zip
cd StarWars
```

### 2. Iniciar Todo

```bash
docker-compose up -d
```

**Eso es todo!** La aplicación está corriendo. 🎉

### 3. Verificar

```bash
# Espera 30 segundos para que todo inicie
curl http://localhost:5000/health
```

Si ves `"status":"Healthy"`, estás listo!

### 4. Explorar

#### Opción A: Navegador (Swagger UI)

Abre en tu navegador: **http://localhost:5000**

Verás una interfaz interactiva donde puedes probar todos los endpoints.

#### Opción B: cURL

```bash
# Listar personajes
curl http://localhost:5000/api/v1/characters

# Buscar a Luke
curl "http://localhost:5000/api/v1/characters/search?name=Luke"

# Agregar a favoritos
curl -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1", "notes": "Mi favorito"}'

# Ver favoritos
curl http://localhost:5000/api/v1/favorites
```

#### Opción C: Cliente de Consola

```bash
cd src/StarWars.Client
dotnet run
```

Sigue el menú interactivo.

## 🎮 Prueba Rápida (1 minuto)

Copia y pega esto en tu terminal:

```bash
# 1. Health check
echo "1. Health Check..."
curl -s http://localhost:5000/health | grep -o '"status":"[^"]*"'

# 2. Listar personajes
echo -e "\n2. Primeros personajes..."
curl -s http://localhost:5000/api/v1/characters | grep -o '"name":"[^"]*"' | head -3

# 3. Buscar a Luke
echo -e "\n3. Buscar a Luke..."
curl -s "http://localhost:5000/api/v1/characters/search?name=Luke" | grep -o '"name":"Luke Skywalker"'

# 4. Agregar favorito
echo -e "\n4. Agregar a favoritos..."
curl -s -X POST http://localhost:5000/api/v1/favorites \
  -H "Content-Type: application/json" \
  -d '{"characterId": "1"}' | grep -o '"name":"[^"]*"'

# 5. Ver favoritos
echo -e "\n5. Favoritos..."
curl -s http://localhost:5000/api/v1/favorites | grep -o '"name":"[^"]*"'

echo -e "\n✅ ¡Todo funciona!"
```

## 📚 Siguiente Paso

- **Documentación completa**: [README.md](README.md)
- **Testing detallado**: [TESTING.md](TESTING.md)
- **Configuración**: [SETUP.md](SETUP.md)

## 🛑 Detener

```bash
docker-compose down
```

## 🔥 Limpiar Todo

```bash
docker-compose down -v
```

## ❓ Problemas

### Puerto 5000 ocupado

```bash
# Editar docker-compose.yml, cambiar:
# ports:
#   - "5001:8080"  # Usar 5001
```

### Docker no arranca

```bash
# Ver logs
docker-compose logs -f
```

### Quiero empezar de cero

```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

---

**¡Listo en 5 minutos!** ⚡

**May the Force be with you!** 🌟

